// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.CosmosDB;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.StorageAdapter;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IConfig config;
        private readonly IStorageAdapterClient storageAdapter;
        private readonly IStorageClient cosmosDb;
        private readonly ITimeSeriesClient timeSeriesClient;
        private readonly ILogger log;

        private const string STORAGE_TYPE_KEY = "StorageType";
        private const string TIME_SERIES_KEY = "tsi";
        private const string TIME_SERIES_EXPLORER_URL_KEY = "TsiExplorerUrl";
        private const string TIME_SERIES_EXPLORER_URL_SEPARATOR_CHAR = ".";

        public StatusController(
            IConfig config,
            IStorageClient cosmosDb,
            IStorageAdapterClient storageAdapter,
            ITimeSeriesClient timeSeriesClient,
            ILogger logger)
        {
            this.config = config;
            this.cosmosDb = cosmosDb;
            this.storageAdapter = storageAdapter;
            this.timeSeriesClient = timeSeriesClient;
            this.log = logger;
        }

        [HttpGet]
        public async Task<StatusApiModel> Get()
        {
            var statusIsOk = true;
            var statusMsg = "Alive and well";
            var errors = new List<string>();

            // Check access to Storage Adapter
            log.Debug("Sending ping", () => new { });
            var storageAdapterStatus = await this.storageAdapter.PingAsync();
            if (!storageAdapterStatus.Item1)
            {
                statusIsOk = false;
                errors.Add("Unable to use key value storage");
            }

            log.Debug("Sending ping to cosmosdb", () => new { });
            // Check connection to CosmosDb
            var cosmosDbStatus = this.cosmosDb.Ping();
            log.Debug("Response cosmosdb", () => new { cosmosDbStatus  });
            if (!cosmosDbStatus.Item1)
            {
                statusIsOk = false;
                errors.Add("Unable to use storage");
            }
            log.Debug("Joining errors", () => new { });
            // Prepare status message
            if (!statusIsOk)
            {
                statusMsg = string.Join(";", errors);
            }
            log.Debug("Joined errors ", () => new { });
            // Prepare response
            var result = new StatusApiModel(statusIsOk, statusMsg);
            result.Dependencies.Add("Key Value Storage", storageAdapterStatus.Item2);
            result.Dependencies.Add("Storage", cosmosDbStatus.Item2);

            log.Debug("TSI ping ", () => new { });
            // Add Time Series Dependencies if needed
            if (this.config.ServicesConfig.StorageType.Equals(
                TIME_SERIES_KEY,
                StringComparison.OrdinalIgnoreCase))
            {
                // Check connection to Time Series Insights
                var timeSeriesStatus = await this.timeSeriesClient.PingAsync();
                log.Debug("TSI response ", () => new { timeSeriesStatus });
                if (!timeSeriesStatus.Item1)
                {
                    statusIsOk = false;
                    errors.Add("Unable to use Time Series Insights");
                }
                log.Debug("TSI response end ", () => new {  });
                result.Dependencies.Add("TimeSeries", timeSeriesStatus.Item2);

                log.Debug("TSI Data ", () => new { });
                // Add Time Series Insights explorer url
                var timeSeriesFqdn = this.config.ServicesConfig.TimeSeriesFqdn;
                var environmentId = timeSeriesFqdn.Substring(0, timeSeriesFqdn.IndexOf(TIME_SERIES_EXPLORER_URL_SEPARATOR_CHAR));
                var explorerUrl = this.config.ServicesConfig.TimeSeriesExplorerUrl +
                    "?environmentId=" + environmentId +
                    "&tid=" + this.config.ServicesConfig.ActiveDirectoryTenant;
                log.Debug("TSI response end ", () => new { explorerUrl });
                result.Properties.Add(TIME_SERIES_EXPLORER_URL_KEY, explorerUrl);
            }

            result.Properties.Add(STORAGE_TYPE_KEY, this.config.ServicesConfig.StorageType);

            this.log.Info("Service status request", () => new { Healthy = statusIsOk, statusMsg });

            return result;
        }
    }
}
