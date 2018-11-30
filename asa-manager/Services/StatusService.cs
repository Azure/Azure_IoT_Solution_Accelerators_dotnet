// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Http;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services
{
    class StatusService : IStatusService
    {
        private readonly ILogger log;
        private readonly IBlobStorageHelper blobStorageHelper;
        private readonly ICosmosDbSql cosmosDbSql;
        private readonly IHttpClient httpClient;
        private readonly IServicesConfig servicesConfig;
        private readonly int timeout;

        private const bool ALLOW_INSECURE_SSL_SERVER = true;

        public StatusService(
            ILogger logger,
            IHttpClient httpClient,
            IBlobStorageHelper blobStorageHelper,
            ICosmosDbSql cosmosDbSql,
            IServicesConfig servicesConfig
            )
        {
            this.log = logger;
            this.httpClient = httpClient;
            this.blobStorageHelper = blobStorageHelper;
            this.cosmosDbSql = cosmosDbSql;
            this.servicesConfig = servicesConfig;
            this.timeout = this.servicesConfig.ConfigServiceTimeout;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();
            string configName = "Config";
            string deviceTelemetryName = "DeviceTelemetry";
            string ioTHubManagerName = "IoTHubManager";

            // Check access to Config
            var configResult = await this.PingServiceAsync(configName, this.servicesConfig.ConfigServiceUrl);
            SetServiceStatus(configName, configResult, result, errors);

            // Check access to Device Telemetry
            var deviceTelemetryResult = await this.PingServiceAsync(
                deviceTelemetryName,
                this.servicesConfig.DeviceTelemetryWebServiceUrl);
            SetServiceStatus(deviceTelemetryName, deviceTelemetryResult, result, errors);

            // Check access to IoTHubManager
            var ioTHubmanagerResult = await this.PingServiceAsync(
                ioTHubManagerName,
                this.servicesConfig.IotHubManagerServiceUrl);
            SetServiceStatus(ioTHubManagerName, ioTHubmanagerResult, result, errors);

            // Check access to Blob
            var blobResult = await this.blobStorageHelper.PingAsync();
            SetServiceStatus("Blob", blobResult, result, errors);

            // Check access to Storage
            var alarmsCosmosDb = this.cosmosDbSql.Initialize(this.servicesConfig.AlarmsCosmosDbConfig);
            var storageResult = await alarmsCosmosDb.PingAsync();
            SetServiceStatus("Storage", storageResult, result, errors);

            // Check access to EventHub
            var eventHubResult = await this.PingEventHubAsync();
            SetServiceStatus("EventHub", eventHubResult, result, errors);

            // Add properties
            result.Properties.Add("ConfigServiceUrl", this.servicesConfig?.ConfigServiceUrl);
            result.Properties.Add("IotHubManagerServiceUrl", this.servicesConfig?.IotHubManagerServiceUrl);
            result.Properties.Add("TelemetryServiceUrl", this.servicesConfig?.DeviceTelemetryWebServiceUrl);
            result.Properties.Add("EventHubName", this.servicesConfig?.EventHubName);
            result.Properties.Add("MessagesStorageType", this.servicesConfig?.MessagesStorageType.ToString());
            result.Properties.Add("AlarmsStorageType", this.servicesConfig?.AlarmsStorageType.ToString());

            this.log.Info(
                "Service status request",
                () => new
                {
                    Healthy = result.Status.IsHealthy,
                    result.Status.Message
                });

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }
            return result;
        }

        private void SetServiceStatus(
            string dependencyName,
            StatusResultServiceModel serviceResult,
            StatusServiceModel result,
            List<string> errors
            )
        {
            if (!serviceResult.IsHealthy)
            {
                errors.Add(dependencyName + " check failed");
                result.Status.IsHealthy = false;
            }
            result.Dependencies.Add(dependencyName, serviceResult);
        }

        private async Task<StatusResultServiceModel> PingServiceAsync(string serviceName, string serviceURL)
        {
            var result = new StatusResultServiceModel(false, $"{serviceName} check failed");
            try
            {
                var response = await this.httpClient.GetAsync(this.PrepareRequest($"{serviceURL}/status"));
                if (response.IsError)
                {
                    result.Message = $"Status code: {response.StatusCode}; Response: {response.Content}";
                }
                else
                {
                    var data = JsonConvert.DeserializeObject<StatusServiceModel>(response.Content);
                    result = data.Status;
                }
            }
            catch (Exception e)
            {
                this.log.Error(result.Message, () => new { e });
            }

            return result;
        }

        private async Task<StatusResultServiceModel> PingEventHubAsync()
        {
            var result = new StatusResultServiceModel(false, "EventHub check failed");
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(
                this.servicesConfig.EventHubConnectionString)
            {
                EntityPath = this.servicesConfig.EventHubName
            };

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(
                connectionStringBuilder.ToString());
            try
            {
                await eventHubClient.GetRuntimeInformationAsync();
                result.Message = "Alive and well!";
                result.IsHealthy = true;
            }
            catch (Exception e)
            {
                this.log.Error(result.Message, () => new { e });
            }
            return result;
        }

        private HttpRequest PrepareRequest(string path)
        {
            var request = new HttpRequest();
            request.AddHeader(HttpRequestHeader.Accept.ToString(), "application/json");
            request.AddHeader(HttpRequestHeader.CacheControl.ToString(), "no-cache");
            request.AddHeader(HttpRequestHeader.Referer.ToString(), "ASA Manager " + this.GetType().FullName);
            request.SetUriFromString(path);
            request.Options.EnsureSuccess = false;
            request.Options.Timeout = this.timeout;
            if (path.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = ALLOW_INSECURE_SSL_SERVER;
            }

            this.log.Debug("Prepare Request", () => new { request });

            return request;
        }
    }
}
