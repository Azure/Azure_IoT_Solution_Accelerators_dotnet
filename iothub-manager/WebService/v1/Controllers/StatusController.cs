// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public class StatusController : Controller
    {
        private readonly IDevices devices;

        // TODO: check if the dependencies are healthy
        public StatusController(IDevices devices)
        {
            // This dependency is not used yet, however just having the instance
            // helps to verify whether DI works
            this.devices = devices;
        }

        /// <summary>Return the service status</summary>
        /// <returns>Status object</returns>
        [HttpGet]
        public StatusApiModel Get()
        {
            return new StatusApiModel(true, "Alive and well");
        }
    }
}
