// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using static Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.DeviceStatusQueries;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDeployments
    {
        Task<DeploymentServiceModel> CreateAsync(DeploymentServiceModel model);
        Task<DeploymentServiceListModel> ListAsync();
        Task<DeploymentServiceModel> GetAsync(string id, bool includeDeviceStatus);
        Task DeleteAsync(string deploymentId);
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeploymentStatus
    {
        Pending, Succeeded, Failed, Unknown
    }

    public class Deployments : IDeployments
    {
        private const int MAX_DEPLOYMENTS = 20;

        private const string DEPLOYMENT_NAME_LABEL = "Name";
        private const string DEPLOYMENT_GROUP_ID_LABEL = "DeviceGroupId";
        private const string DEPLOYMENT_GROUP_NAME_LABEL = "DeviceGroupName";
        private const string DEPLOYMENT_PACKAGE_NAME_LABEL = "PackageName";
        private const string RM_CREATED_LABEL = "RMDeployment";

        private const string DEVICE_GROUP_ID_PARAM = "deviceGroupId";
        private const string DEVICE_GROUP_QUERY_PARAM = "deviceGroupQuery";
        private const string NAME_PARAM = "name";
        private const string PACKAGE_CONTENT_PARAM = "packageContent";
        private const string CONFIG_TYPE_PARAM = "configType";
        private const string PRIORITY_PARAM = "priority";

        private const string DEVICE_ID_KEY = "DeviceId";
        private const string EDGE_MANIFEST_SCHEMA = "schemaVersion";

        private RegistryManager registry;
        private string ioTHubHostName;
        private readonly ILogger log;

        public Deployments(
            IServicesConfig config,
            ILogger logger)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            IoTHubConnectionHelper.CreateUsingHubConnectionString(config.IoTHubConnString, (conn) =>
            {
                this.registry = RegistryManager.CreateFromConnectionString(conn);
                this.ioTHubHostName = IotHubConnectionStringBuilder.Create(conn).HostName;
            });

            this.log = logger;
        }

        public Deployments(
            RegistryManager registry,
            string ioTHubHostName)
        {
            this.registry = registry;
            this.ioTHubHostName = ioTHubHostName;
        }

        /// <summary>
        /// Schedules a deployment of the provided package, to the given group.
        /// </summary>
        /// <returns>Scheduled deployment</returns>
        public async Task<DeploymentServiceModel> CreateAsync(DeploymentServiceModel model)
        {
            if (string.IsNullOrEmpty(model.DeviceGroupId))
            {
                throw new ArgumentNullException(DEVICE_GROUP_ID_PARAM);
            }

            if (string.IsNullOrEmpty(model.DeviceGroupQuery))
            {
                throw new ArgumentNullException(DEVICE_GROUP_QUERY_PARAM);
            }

            if (string.IsNullOrEmpty(model.Name))
            {
                throw new ArgumentNullException(NAME_PARAM);
            }

            if (string.IsNullOrEmpty(model.PackageContent))
            {
                throw new ArgumentNullException(PACKAGE_CONTENT_PARAM);
            }

            if (model.PackageType.Equals(PackageType.DeviceConfiguration) 
                && string.IsNullOrEmpty(model.ConfigType))
            {
                throw new ArgumentNullException(CONFIG_TYPE_PARAM);
            }

            if (model.Priority < 0)
            {
                throw new ArgumentOutOfRangeException(PRIORITY_PARAM,
                    model.Priority,
                    "The priority provided should be 0 or greater");
            }

            var configuration = ConfigurationsHelper.ToHubConfiguration(model);
            // TODO: Add specific exception handling when exception types are exposed
            // https://github.com/Azure/azure-iot-sdk-csharp/issues/649
            return new DeploymentServiceModel(await this.registry.AddConfigurationAsync(configuration));
        }

        /// <summary>
        /// Retrieves all deployments that have been scheduled on the iothub.
        /// Only deployments which were created by RM will be returned.
        /// </summary>
        /// <returns>All scheduled deployments with RMDeployment label</returns>
        public async Task<DeploymentServiceListModel> ListAsync()
        {
            // TODO: Currently they only support 20 deployments
            var deployments = await this.registry.GetConfigurationsAsync(MAX_DEPLOYMENTS);

            if (deployments == null)
            {
                throw new ResourceNotFoundException($"No deployments found for {this.ioTHubHostName} hub.");
            }

            List<DeploymentServiceModel> serviceModelDeployments = 
                deployments.Where(this.CheckIfDeploymentWasMadeByRM)
                           .Select(config => new DeploymentServiceModel(config))
                           .OrderBy(conf => conf.Name)
                           .ToList();

            return new DeploymentServiceListModel(serviceModelDeployments);
        }

        /// <summary>
        /// Retrieve information on a single deployment given its id.
        /// If includeDeviceStatus is included additional queries are created to retrieve the status of
        /// the deployment per device.
        /// </summary>
        /// <returns>Deployment for the given id</returns>
        public async Task<DeploymentServiceModel> GetAsync(string deploymentId, bool includeDeviceStatus = false)
        {
            if (string.IsNullOrEmpty(deploymentId))
            {
                throw new ArgumentNullException(nameof(deploymentId));
            }

            var deployment = await this.registry.GetConfigurationAsync(deploymentId);

            if (deployment == null)
            {
                throw new ResourceNotFoundException($"Deployment with id {deploymentId} not found.");
            }

            if (!this.CheckIfDeploymentWasMadeByRM(deployment))
            {
                throw new ResourceNotSupportedException($"Deployment with id {deploymentId}" + @" was 
                                                        created externally and therefore not supported");
            }

            IDictionary<string, DeploymentStatus> deviceStatuses = this.GetDeviceStatuses(deployment);

            return new DeploymentServiceModel(deployment)
            {
                DeploymentMetrics =
                {
                    DeviceMetrics = CalculateDeviceMetrics(deviceStatuses),
                    DeviceStatuses = includeDeviceStatus ? deviceStatuses : null
                }
            };
        }

        /// <summary>
        /// Delete a given deployment by id.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(string deploymentId)
        {
            if(string.IsNullOrEmpty(deploymentId))
            {
                throw new ArgumentNullException(nameof(deploymentId));
            }

            await this.registry.RemoveConfigurationAsync(deploymentId);
        }

        private bool CheckIfDeploymentWasMadeByRM(Configuration conf)
        {
            return conf.Labels != null &&
                   conf.Labels.ContainsKey(RM_CREATED_LABEL) &&
                   bool.TryParse(conf.Labels[RM_CREATED_LABEL], out var res) && res;
        }

        private IDictionary<string, DeploymentStatus> GetDeviceStatuses(Configuration deployment)
        {
            string deploymentType = null;
            if (ConfigurationsHelper.IsEdgeDeployment(deployment))
            {
                deploymentType = PackageType.EdgeManifest.ToString();
            }
            else
            {
                deploymentType = PackageType.DeviceConfiguration.ToString();
            }

            deployment.Labels.TryGetValue(ConfigurationsHelper.CONFIG_TYPE_LABEL, out string configType);
            IDictionary<QueryType, String> Queries = GetQueries(deploymentType, configType);

            string deploymentId = deployment.Id;
            var appliedDevices = this.GetDevicesInQuery(Queries[QueryType.APPLIED], deploymentId);

            var deviceWithStatus = new Dictionary<string, DeploymentStatus>();

            if (!(ConfigurationsHelper.IsEdgeDeployment(deployment)) &&
                    !(configType.Equals(ConfigType.Firmware.ToString())))
            {
                foreach (var devices in appliedDevices)
                {
                    deviceWithStatus.Add(devices, DeploymentStatus.Unknown);
                }

                return deviceWithStatus;
            }

            var successfulDevices = this.GetDevicesInQuery(Queries[QueryType.SUCCESSFUL], deploymentId);
            var failedDevices = this.GetDevicesInQuery(Queries[QueryType.FAILED], deploymentId);

            foreach (var successfulDevice in successfulDevices)
            {
                deviceWithStatus.Add(successfulDevice, DeploymentStatus.Succeeded);
            }

            foreach (var failedDevice in failedDevices)
            {
                deviceWithStatus.Add(failedDevice, DeploymentStatus.Failed);
            }

            foreach (var device in appliedDevices)
            {
                if (!successfulDevices.Contains(device) && !failedDevices.Contains(device))
                {
                    deviceWithStatus.Add(device, DeploymentStatus.Pending);
                }
            }

            return deviceWithStatus;
        }

        private HashSet<string> GetDevicesInQuery(string hubQuery, string deploymentId)
        {
            var query = string.Format(hubQuery, deploymentId);
            var queryResponse = this.registry.CreateQuery(query);
            var deviceIds = new HashSet<string>();

            try
            {
                while (queryResponse.HasMoreResults)
                {
                    // TODO: Add pagination with queryOptions
                    var resultSet = queryResponse.GetNextAsJsonAsync();
                    foreach (var result in resultSet.Result)
                    {
                        var deviceId = JToken.Parse(result)[DEVICE_ID_KEY];
                        deviceIds.Add(deviceId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                this.log.Error($"Error getting status of devices in query {query}", () => new { ex.Message });
            }

            return deviceIds;
        }

        private IDictionary<DeploymentStatus, long> CalculateDeviceMetrics(
            IDictionary<string, 
            DeploymentStatus> deviceStatuses)
        {
            if (deviceStatuses == null)
            {
                return null;
            }

            IDictionary<DeploymentStatus, long> deviceMetrics = new Dictionary<DeploymentStatus, long>();

            deviceMetrics[DeploymentStatus.Succeeded] = deviceStatuses.Where(item =>
                                                            item.Value == DeploymentStatus.Succeeded).LongCount();

            deviceMetrics[DeploymentStatus.Failed] = deviceStatuses.Where(item =>
                                                            item.Value == DeploymentStatus.Failed).LongCount();

            deviceMetrics[DeploymentStatus.Pending] = deviceStatuses.Where(item =>
                                                            item.Value == DeploymentStatus.Pending).LongCount();

            return deviceMetrics;
        }
    }
}