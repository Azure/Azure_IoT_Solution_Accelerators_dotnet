// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class DevicePropertiesController : Controller
    {
        private readonly IDeviceProperties deviceProperties;

        public DevicePropertiesController(IDeviceProperties deviceProperties)
        {
            this.deviceProperties = deviceProperties;
        }

        [HttpGet]
        public async Task<DevicePropertiesApiModel> GetAsync()
        {
            return new DevicePropertiesApiModel(await this.deviceProperties.GetListAsync());
        }
    }
}
