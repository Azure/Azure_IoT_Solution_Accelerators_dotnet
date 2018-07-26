// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Http;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent
{
    public interface IDevicesClient
    {
        Task<IEnumerable<string>> GetListAsync(IEnumerable<DeviceGroupConditionApiModel> conditions);
    }

    public class DevicesClient : IDevicesClient
    {
        private readonly ILogger logger;
        private readonly IHttpClient httpClient;
        private readonly string baseUrl;

        public DevicesClient(
            IHttpClient httpClient,
            IServicesConfig config,
            ILogger logger)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.baseUrl = $"{config.IotHubManagerServiceUrl}/devices";
        }

        /**
         * Given a device group definition, returns a list of the device ids in that group
         */
        public async Task<IEnumerable<string>> GetListAsync(IEnumerable<DeviceGroupConditionApiModel> conditions)
        {
            try
            {
                var query = JsonConvert.SerializeObject(conditions);

                var deviceList = await this.httpClient.GetJsonAsync<DeviceListApiModel>($"{this.baseUrl}?query={query}", $"devices by query {query}");
                return deviceList.Items.Select(d => d.Id);
            }
            catch (Exception e)
            {
                this.logger.Warn("Failed to get list of devices", () => new { e });
                throw new ExternalDependencyException("Unable to get list of devices", e);
            }
        }
    }
}
