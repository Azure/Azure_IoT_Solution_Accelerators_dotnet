// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public interface IAgent
    {
        Task RunAsync(CancellationToken runState);
    }

    public class Agent : IAgent
    {
        private readonly ILogger logger;
        private readonly IServicesConfig servicesConfig;
        private readonly IBlobStorageConfig blobStorageConfig;
        private readonly IEventProcessorFactory notificationEventProcessorFactory;
        private readonly IEventProcessorHostWrapper eventProcessorHostWrapper;

        private CancellationToken runState;

        public Agent(ILogger logger,
            IServicesConfig servicesConfig,
            IBlobStorageConfig blobStorageConfig,
            IEventProcessorHostWrapper eventProcessorHostWrapper,
            IEventProcessorFactory notificationEventProcessorFactory)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
            this.blobStorageConfig = blobStorageConfig;
            this.notificationEventProcessorFactory = notificationEventProcessorFactory;
            this.eventProcessorHostWrapper = eventProcessorHostWrapper;
        }

        public async Task RunAsync(CancellationToken runState)
        {
            this.runState = runState;
            this.logger.Info("Notification System Running", () => { });
            await this.SetupEventHub(this.runState);
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
                    await this.eventProcessorHostWrapper.RegisterEventProcessorFactoryAsync(eventProcessorHost, this.notificationEventProcessorFactory);
                }
                catch (Exception e)
                {
                    this.logger.Error("Received error setting up event hub. Will not receive alarm notification from the eventhub", () => new { e });
                }
            }
        }
    }
}
