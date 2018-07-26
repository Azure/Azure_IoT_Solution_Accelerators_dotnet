// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Concurrency;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.EventHub;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent
{
    public interface IAgent
    {
        Task RunAsync(CancellationToken runState);
    }

    public class Agent : IAgent
    {
        // ASA cannot have new reference data more than once per minute
        private const int CHECK_INTERVAL_MSECS = 60000;

        private readonly ILogger log;
        private readonly IDeviceGroupsWriter deviceGroupsWriter;
        private readonly IDeviceGroupsClient deviceGroupsClient;
        private readonly IEventHubStatus eventHubStatus;
        private readonly IEventProcessorHostWrapper eventProcessorHostWrapper;
        private readonly IEventProcessorFactory deviceEventProcessorFactory;
        private readonly IServicesConfig servicesConfig;
        private readonly IBlobStorageConfig blobStorageConfig;
        private readonly Dictionary<string, string> deviceGroupDefinitionDictionary;
        private readonly IThreadWrapper thread;
        private Dictionary<string, IEnumerable<string>> mostRecentMapping;

        public Agent(
           IDeviceGroupsWriter deviceGroupsWriter,
           IDeviceGroupsClient deviceGroupsClient,
           IEventProcessorHostWrapper eventProcessorHostWrapper,
           IEventProcessorFactory deviceEventProcessorFactory,
           IEventHubStatus eventHubStatus,
           IServicesConfig servicesConfig,
           IBlobStorageConfig blobStorageConfig,
           ILogger logger,
           IThreadWrapper thread)
        {
            this.log = logger;
            this.deviceGroupsWriter = deviceGroupsWriter;
            this.deviceGroupsClient = deviceGroupsClient;
            this.eventProcessorHostWrapper = eventProcessorHostWrapper;
            this.deviceEventProcessorFactory = deviceEventProcessorFactory;
            this.eventHubStatus = eventHubStatus;
            this.servicesConfig = servicesConfig;
            this.blobStorageConfig = blobStorageConfig;
            this.deviceGroupDefinitionDictionary = new Dictionary<string, string>();
            this.thread = thread;
        }

        public async Task RunAsync(CancellationToken runState)
        {
            this.log.Info("Device Groups Agent running", () => { });

            // ensure will do initial write even if there are no device group definitions
            bool forceWrite = true;

            // IotHub has some latency between reporting a device is created/updated and when
            // the API returns the updates. This flag will tell the service to write
            // again a minute after changes have been seen,
            // to ensures if there are updates they are not missed.
            bool previousEventHubSeenChanges = this.eventHubStatus.SeenChanges;
            
            await this.SetupEventHub(runState);
            
            this.mostRecentMapping = null;

            while (!runState.IsCancellationRequested)
            {
                try
                {
                    // check device groups
                    DeviceGroupListApiModel deviceGroupList = await this.deviceGroupsClient.GetDeviceGroupsAsync();
                    bool deviceGroupsChanged = this.DidDeviceGroupDefinitionsChange(deviceGroupList);

                    if (forceWrite || deviceGroupsChanged || this.eventHubStatus.SeenChanges || previousEventHubSeenChanges)
                    {
                        previousEventHubSeenChanges = this.eventHubStatus.SeenChanges;

                        // set status before update so if message is received during update, will update again in a minute
                        this.eventHubStatus.SeenChanges = false;

                        // update device group definition dictionary
                        this.UpdateDeviceGroupDefinitionDictionary(deviceGroupList, deviceGroupsChanged);

                        // get device group -> devices mapping and write to file if it has changed
                        await this.GetAndWriteDeviceGroupsMapping(deviceGroupList);

                        forceWrite = false;
                    }
                }
                catch (Exception e)
                {
                    this.log.Error("Received error updating device to device group mapping", () => new { e });
                    forceWrite = true;
                }

                this.thread.Sleep(CHECK_INTERVAL_MSECS);
            }
        }

        private async Task SetupEventHub(CancellationToken runState)
        {
            if (!runState.IsCancellationRequested)
            {
                try
                {
                    string storageConnectionString =
                        $"DefaultEndpointsProtocol=https;AccountName={this.blobStorageConfig.AccountName};AccountKey={this.blobStorageConfig.AccountKey};EndpointSuffix={this.blobStorageConfig.EndpointSuffix}";
                    var eventProcessorHost = this.eventProcessorHostWrapper.CreateEventProcessorHost(
                        this.servicesConfig.EventHubName,
                        PartitionReceiver.DefaultConsumerGroupName,
                        this.servicesConfig.EventHubConnectionString,
                        storageConnectionString,
                        this.blobStorageConfig.EventHubContainer);
                    await this.eventProcessorHostWrapper.RegisterEventProcessorFactoryAsync(eventProcessorHost, this.deviceEventProcessorFactory);
                }
                catch (Exception e)
                {
                    this.log.Error("Received error setting up event hub. Will not receive updates from devices", () => new { e });
                }
            }
        }

        private void UpdateDeviceGroupDefinitionDictionary(DeviceGroupListApiModel deviceGroupList, bool deviceGroupsChanged)
        {
            if (deviceGroupsChanged)
            {
                this.deviceGroupDefinitionDictionary.Clear();
                foreach (DeviceGroupApiModel deviceGroup in deviceGroupList.Items)
                {
                    this.deviceGroupDefinitionDictionary[deviceGroup.Id] = deviceGroup.ETag;
                }
            }
        }

        private bool DidDeviceGroupDefinitionsChange(DeviceGroupListApiModel newDeviceGroupList)
        {
            if (newDeviceGroupList.Items.Count() != this.deviceGroupDefinitionDictionary.Keys.Count)
            {
                return true;
            }

            foreach (DeviceGroupApiModel deviceGroup in newDeviceGroupList.Items)
            {
                if (!this.deviceGroupDefinitionDictionary.ContainsKey(deviceGroup.Id) ||
                    !this.deviceGroupDefinitionDictionary[deviceGroup.Id].Equals(deviceGroup.ETag))
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * Given list of device groups, query for device group -> device(s) mapping.
         * If this mapping is different from previous known mapping, or have no previous
         * known mapping, write mapping to reference data
         */
        private async Task GetAndWriteDeviceGroupsMapping(DeviceGroupListApiModel deviceGroupList)
        {
            var deviceGroupMapping = await this.deviceGroupsClient.GetGroupToDevicesMappingAsync(deviceGroupList);
            if (this.mostRecentMapping == null ||
                !this.AreDictionariesTheSame(this.mostRecentMapping, deviceGroupMapping))
            {
                await this.deviceGroupsWriter.ExportMapToReferenceDataAsync(deviceGroupMapping, DateTimeOffset.UtcNow);
                this.mostRecentMapping = deviceGroupMapping;
            }
        }

        /**
         * Given two dictionaries of <string, IEnumerable<string>>,
         * return false if the dictionaries
         * have different contents and true if they are the same.
         * Ordering of IEnumerables does not impact equality.
         * If both are null will return true. If one is null and the other is not
         * will return false.
         */
        private bool AreDictionariesTheSame(
            Dictionary<string, IEnumerable<string>> firstDictionary,
            Dictionary<string, IEnumerable<string>> secondDictionary)
        {
            if (firstDictionary == null && secondDictionary == null)
            {
                return true;
            }
            else if (firstDictionary == null || secondDictionary == null)
            {
                return false;
            }

            var firstKeys = firstDictionary.Keys;
            var secondKeys = secondDictionary.Keys;

            if (firstKeys.Count != secondKeys.Count)
            {
                return false;
            }

            if (firstKeys.Except(secondKeys).Any())
            {
                return false;
            }

            foreach (string key in firstKeys)
            {
                var previousList = firstDictionary[key].ToList();
                var nextList = secondDictionary[key].ToList();
                if (previousList.Count != nextList.Count ||
                    previousList.Except(nextList).Any())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
