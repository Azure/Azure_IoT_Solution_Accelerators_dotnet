// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class MethodParameterApiModel
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "ResponseTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? ResponseTimeout { get; set; }

        [JsonProperty(PropertyName = "ConnectionTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? ConnectionTimeout { get; set; }

        [JsonProperty(PropertyName = "JsonPayload")]
        public string JsonPayload { get; set; }

        public MethodParameterApiModel()
        {
        }

        public MethodParameterApiModel(MethodParameterServiceModel serviceModel)
        {
            if (serviceModel != null)
            {
                this.Name = serviceModel.Name;
                this.ResponseTimeout = serviceModel.ResponseTimeout;
                this.ConnectionTimeout = serviceModel.ConnectionTimeout;
                this.JsonPayload = serviceModel.JsonPayload;
            }
        }

        public MethodParameterServiceModel ToServiceModel()
        {
            return new MethodParameterServiceModel()
            {
                Name = this.Name,
                ResponseTimeout = this.ResponseTimeout,
                ConnectionTimeout = this.ConnectionTimeout,
                JsonPayload = this.JsonPayload
            };
        }
    }
}
