// Copyright (c) Microsoft. All rights reserved.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public class DevicesController : Controller
    {
        const string CONTINUATION_TOKEN_NAME = "x-ms-continuation";

        private readonly IDevices devices;
        private readonly IDeviceProperties deviceProperties;
        private readonly IDeviceService deviceService;

        public DevicesController(IDevices devices, IDeviceService deviceService, IDeviceProperties deviceProperties)
        {
            this.deviceProperties = deviceProperties;
            this.devices = devices;
            this.deviceService = deviceService;
        }

        /// <summary>Get a list of devices</summary>
        /// <returns>List of devices</returns>
        [HttpGet]
        [Authorize("CreateDevices")]
        public async Task<DeviceListApiModel> GetDevicesAsync([FromQuery] string query)
        {
            string continuationToken = string.Empty;
            if (this.Request.Headers.ContainsKey(CONTINUATION_TOKEN_NAME))
            {
                continuationToken = this.Request.Headers[CONTINUATION_TOKEN_NAME].FirstOrDefault();
            }

            return new DeviceListApiModel(await this.devices.GetListAsync(query, continuationToken));
        }

        [HttpPost("query")]
        public async Task<DeviceListApiModel> QueryDevicesAsync([FromBody] string query)
        {
            string continuationToken = string.Empty;
            if (this.Request.Headers.ContainsKey(CONTINUATION_TOKEN_NAME))
            {
                continuationToken = this.Request.Headers[CONTINUATION_TOKEN_NAME].FirstOrDefault();
            }

            return new DeviceListApiModel(await this.devices.GetListAsync(query, continuationToken));
        }

        /// <summary>Get one device</summary>
        /// <param name="id">Device Id</param>
        /// <returns>Device information</returns>
        [HttpGet("{id}")]
        public async Task<DeviceRegistryApiModel> GetDeviceAsync(string id)
        {
            return new DeviceRegistryApiModel(await this.devices.GetAsync(id));
        }

        /// <summary>Create one device</summary>
        /// <param name="device">Device information</param>
        /// <returns>Device information</returns>
        [HttpPost]
        [Authorize("CreateDevices")]
        public async Task<DeviceRegistryApiModel> PostAsync([FromBody] DeviceRegistryApiModel device)
        {
            return new DeviceRegistryApiModel(await this.devices.CreateAsync(device.ToServiceModel()));
        }

        /// <summary>Update device twin</summary>
        /// <param name="id">Device Id</param>
        /// <param name="device">Device information</param>
        /// <returns>Device information</returns>
        [HttpPut("{id}")]
        [Authorize("UpdateDevices")]
        public async Task<DeviceRegistryApiModel> PutAsync(string id, [FromBody] DeviceRegistryApiModel device)
        {
            DevicePropertyDelegate updateListDelegate = new DevicePropertyDelegate(this.deviceProperties.UpdateListAsync);
            return new DeviceRegistryApiModel(await this.devices.CreateOrUpdateAsync(device.ToServiceModel(), updateListDelegate));
        }

        /// <summary>Remove device</summary>
        /// <param name="id">Device Id</param>
        [HttpDelete("{id}")]
        [Authorize("DeleteDevices")]
        public async Task DeleteAsync(string id)
        {
            await this.devices.DeleteAsync(id);
        }

        /// <summary>
        /// Interactively invokes a method on device
        /// </summary>
        /// <param name="id">Device Id</param>
        /// <param name="parameter">Device method parameters (passthrough to device)</param>
        /// <returns></returns>
        [HttpPost("{id}/methods")]
        [Authorize("CreateJobs")]
        public async Task<MethodResultApiModel> InvokeDeviceMethodAsync(string id, [FromBody] MethodParameterApiModel parameter)
        {
            return new MethodResultApiModel(await this.deviceService.InvokeDeviceMethodAsync(id, parameter.ToServiceModel()));
        }
    }
}
