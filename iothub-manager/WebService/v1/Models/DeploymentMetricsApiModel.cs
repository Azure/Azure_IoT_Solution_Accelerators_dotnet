// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class DeploymentMetricsApiModel
    {
        private const string APPLIED_METRICS_KEY = "appliedCount";
        private const string TARGETED_METRICS_KEY = "targetedCount";
        private const string SUCCESSFUL_METRICS_KEY = "reportedSuccessfulCount";
        private const string FAILED_METRICS_KEY = "reportedFailedCount";
        private const string PENDING_METRICS_KEY = "pendingCount";

        [JsonProperty(PropertyName = "SystemMetrics")]
        public IDictionary<string, long> SystemMetrics { get; set; }
      
        [JsonProperty(PropertyName = "CustomMetrics")]
        public IDictionary<string, long> CustomMetrics { get; set; }

        [JsonProperty(PropertyName = "DeviceStatuses")]
        public IDictionary<string, DeploymentStatus> DeviceStatuses { get; set; }

        public DeploymentMetricsApiModel(DeploymentMetricsServiceModel metricsServiceModel)
        {
            this.SystemMetrics = new Dictionary<string, long>();

            this.SystemMetrics[APPLIED_METRICS_KEY] = 0;
            this.SystemMetrics[TARGETED_METRICS_KEY] = 0;

            if (metricsServiceModel == null) return;

            this.CustomMetrics = metricsServiceModel.CustomMetrics;
            this.SystemMetrics = metricsServiceModel.SystemMetrics != null && metricsServiceModel.SystemMetrics.Count > 0 ? 
                metricsServiceModel.SystemMetrics : this.SystemMetrics;
            this.DeviceStatuses = metricsServiceModel.DeviceStatuses;

            if (metricsServiceModel.DeviceMetrics != null)
            {
                this.SystemMetrics[SUCCESSFUL_METRICS_KEY] = 
                    metricsServiceModel.DeviceMetrics[DeploymentStatus.Succeeded];
                this.SystemMetrics[FAILED_METRICS_KEY] = 
                    metricsServiceModel.DeviceMetrics[DeploymentStatus.Failed];
                this.SystemMetrics[PENDING_METRICS_KEY] = 
                    metricsServiceModel.DeviceMetrics[DeploymentStatus.Pending];
            }

            if (this.CustomMetrics != null)
            {
                // Override System metrics if custom metric contain same metrics
                if (this.CustomMetrics.ContainsKey(SUCCESSFUL_METRICS_KEY))
                {
                    this.SystemMetrics[SUCCESSFUL_METRICS_KEY] =
                        this.CustomMetrics[SUCCESSFUL_METRICS_KEY];
                    this.CustomMetrics.Remove(SUCCESSFUL_METRICS_KEY);
                }

                if (this.CustomMetrics.ContainsKey(FAILED_METRICS_KEY))
                {
                    this.SystemMetrics[FAILED_METRICS_KEY] =
                        this.CustomMetrics[FAILED_METRICS_KEY];
                    this.CustomMetrics.Remove(FAILED_METRICS_KEY);
                }

                if (this.CustomMetrics.ContainsKey(PENDING_METRICS_KEY))
                {
                    this.SystemMetrics[PENDING_METRICS_KEY] =
                        this.CustomMetrics[PENDING_METRICS_KEY];
                    this.CustomMetrics.Remove(PENDING_METRICS_KEY);
                }
            }
        }
    }
}
