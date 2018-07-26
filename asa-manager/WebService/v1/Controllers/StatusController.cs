// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public sealed class StatusController : Controller
    {
        private const string SERVICE_IS_HEALTHY = "Alive and well";

        private readonly ILogger log;

        public StatusController(ILogger logger)
        {
            this.log = logger;
        }

        [HttpGet]
        public StatusApiModel Get()
        {
            var result = new StatusApiModel();
            var statusMsg = SERVICE_IS_HEALTHY;
            var errors = new List<string>();
            var statusIsOk = true;

            result.SetStatus(statusIsOk, statusMsg);

            this.log.Info("Service status request", () => new
            {
                Healthy = statusIsOk,
                statusMsg
            });

            return result;
        }
    }
}
