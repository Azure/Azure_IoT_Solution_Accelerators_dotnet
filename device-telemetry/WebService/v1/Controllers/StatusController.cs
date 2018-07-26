// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.StorageAdapter;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IStorageAdapterClient storageAdapter;
        private readonly IStorageClient documentDb;
        private readonly ILogger log;

        public StatusController(
            IStorageClient documentDb,
            IStorageAdapterClient storageAdapter,
            ILogger logger)
        {
            this.documentDb = documentDb;
            this.storageAdapter = storageAdapter;
            this.log = logger;
        }

        [HttpGet]
        public async Task<StatusApiModel> Get()
        {
            var statusIsOk = true;
            var statusMsg = "Alive and well";
            var errors = new List<string>();

            // Check access to Storage Adapter
            var storageAdapterStatus = await this.storageAdapter.PingAsync();
            if (!storageAdapterStatus.Item1)
            {
                statusIsOk = false;
                errors.Add("Unable to use key value storage");
            }

            // Check connection to DocumentDb
            var documentDbStatus = this.documentDb.Ping();
            if (!documentDbStatus.Item1)
            {
                statusIsOk = false;
                errors.Add("Unable to use storage");
            }

            // Prepare status message
            if (!statusIsOk)
            {
                statusMsg = string.Join(";", errors);
            }

            // Prepare response
            var result = new StatusApiModel(statusIsOk, statusMsg);
            result.Dependencies.Add("Key Value Storage", storageAdapterStatus.Item2);
            result.Dependencies.Add("Storage", documentDbStatus.Item2);

            this.log.Info("Service status request", () => new { Healthy = statusIsOk, statusMsg });

            return result;
        }
    }
}
