// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class JobApiModel
    {
        [JsonProperty("JobId")]
        public string JobId { get; set; }

        [JsonProperty(PropertyName = "QueryCondition", NullValueHandling = NullValueHandling.Ignore)]
        public string QueryCondition { get; set; }

        [JsonProperty(PropertyName = "CreatedTimeUtc", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreatedTimeUtc { get; set; }

        [JsonProperty(PropertyName = "StartTimeUtc", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartTimeUtc { get; set; }

        [JsonProperty(PropertyName = "EndTimeUtc", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndTimeUtc { get; set; }

        [JsonProperty(PropertyName = "MaxExecutionTimeInSeconds", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxExecutionTimeInSeconds { get; set; }

        [JsonProperty(PropertyName = "Type")]
        public JobType Type { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public JobStatus Status { get; set; }

        [JsonProperty(PropertyName = "MethodParameter", NullValueHandling = NullValueHandling.Ignore)]
        public MethodParameterApiModel MethodParameter { get; set; }

        [JsonProperty(PropertyName = "UpdateTwin", NullValueHandling = NullValueHandling.Ignore)]
        public JobUpdateTwinApiModel UpdateTwin { get; set; }

        [JsonProperty(PropertyName = "FailureReason", NullValueHandling = NullValueHandling.Ignore)]
        public string FailureReason { get; set; }

        [JsonProperty(PropertyName = "StatusMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string StatusMessage { get; set; }

        [JsonProperty(PropertyName = "ResultStatistics", NullValueHandling = NullValueHandling.Ignore)]
        public JobStatistics ResultStatistics { get; set; }

        [JsonProperty(PropertyName = "Devices", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DeviceJobApiModel> Devices { get; set; }

        public JobApiModel()
        {
        }

        public JobApiModel(JobServiceModel serviceModel)
        {
            if (serviceModel != null)
            {
                this.JobId = serviceModel.JobId;
                this.QueryCondition = serviceModel.QueryCondition;
                this.CreatedTimeUtc = serviceModel.CreatedTimeUtc;
                this.StartTimeUtc = serviceModel.StartTimeUtc;
                this.EndTimeUtc = serviceModel.EndTimeUtc;
                this.MaxExecutionTimeInSeconds = serviceModel.MaxExecutionTimeInSeconds;
                this.Type = serviceModel.Type;
                this.Status = serviceModel.Status;
                this.MethodParameter = serviceModel.MethodParameter == null ? null : new MethodParameterApiModel(serviceModel.MethodParameter);
                this.UpdateTwin = serviceModel.UpdateTwin == null ? null : new JobUpdateTwinApiModel(null, serviceModel.UpdateTwin);
                this.FailureReason = serviceModel.FailureReason;
                this.StatusMessage = serviceModel.StatusMessage;
                this.ResultStatistics = serviceModel.ResultStatistics;
                this.Devices = serviceModel.Devices?.Select(j => new DeviceJobApiModel(j));
            }
        }
    }
}
