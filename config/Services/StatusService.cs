// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    class StatusService : IStatusService
    {
        private readonly ILogger log;
        private readonly IHttpClient httpClient;
        private readonly IServicesConfig servicesConfig;
        private readonly int timeoutMS = 10000;

        private const bool ALLOW_INSECURE_SSL_SERVER = true;

        public StatusService(
            ILogger logger,
            IHttpClient httpClient,
            IServicesConfig servicesConfig)
        {
            this.log = logger;
            this.httpClient = httpClient;
            this.servicesConfig = servicesConfig;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();
            string storageAdapterName = "StorageAdapter";
            string deviceTelemetryName = "DeviceTelemetry";
            string deviceSimulationName = "DeviceSimulation";
            string authName = "Auth";

            // Check access to StorageAdapter
            var storageAdapterResult = await this.PingServiceAsync(
                storageAdapterName,
                this.servicesConfig.StorageAdapterApiUrl);
            SetServiceStatus(storageAdapterName, storageAdapterResult, result, errors);

            // Check access to Device Telemetry
            var deviceTelemetryResult = await this.PingServiceAsync(
                deviceTelemetryName,
                this.servicesConfig.TelemetryApiUrl);
            SetServiceStatus(deviceTelemetryName, deviceTelemetryResult, result, errors);

            // Check access to DeviceSimulation

            /* TODO: Remove PingSimulationAsync and use PingServiceAsync once DeviceSimulation has started 
             * using the new 'Status' model */
            var deviceSimulationResult = await this.PingSimulationAsync(
                deviceSimulationName,
                this.servicesConfig.DeviceSimulationApiUrl);
            SetServiceStatus(deviceSimulationName, deviceSimulationResult, result, errors);

            // Check access to Auth
            var authResult = await this.PingServiceAsync(
                authName,
                this.servicesConfig.DeviceSimulationApiUrl);
            SetServiceStatus(authName, authResult, result, errors);

            // Add properties
            result.Properties.Add("DeviceSimulationApiUrl", this.servicesConfig?.DeviceSimulationApiUrl);
            result.Properties.Add("StorageAdapterApiUrl", this.servicesConfig?.StorageAdapterApiUrl);
            result.Properties.Add("UserManagementApiUrl", this.servicesConfig?.UserManagementApiUrl);
            result.Properties.Add("TelemetryApiUrl", this.servicesConfig?.TelemetryApiUrl);
            result.Properties.Add("SeedTemplate", this.servicesConfig?.SeedTemplate);
            result.Properties.Add("SolutionType", this.servicesConfig?.SolutionType);

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
            List<string> errors)
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
                if (!response.IsSuccessStatusCode)
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

        private async Task<StatusResultServiceModel> PingSimulationAsync(string serviceName, string serviceURL)
        {
            var result = new StatusResultServiceModel(false, $"{serviceName} check failed");
            try
            {
                var response = await this.httpClient.GetAsync(this.PrepareRequest($"{serviceURL}/status"));
                if (!response.IsSuccessStatusCode)
                {
                    result.Message = $"Status code: {response.StatusCode}; Response: {response.Content}";
                }
                else
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                    result.Message = data["Status"].ToString();
                    result.IsHealthy = data["Status"].ToString().StartsWith("OK:");
                }
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
            request.AddHeader(HttpRequestHeader.Referer.ToString(), "Config " + this.GetType().FullName);
            request.SetUriFromString(path);
            request.Options.EnsureSuccess = false;
            request.Options.Timeout = this.timeoutMS;
            if (path.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = ALLOW_INSECURE_SSL_SERVER;
            }

            this.log.Debug("Prepare Request", () => new { request });

            return request;
        }
    }
}
