// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class MessageApiModel
    {
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";
        private DateTimeOffset time;

        [JsonProperty(PropertyName = "DeviceId")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "Time")]
        public string Time => this.time.ToString(DATE_FORMAT);

        [JsonProperty(PropertyName = "Data")]
        public JObject Data { get; set; }

        public MessageApiModel(
            string deviceId,
            DateTimeOffset time,
            JObject data)
        {
            this.DeviceId = deviceId;
            this.time = time;
            this.Data = data;
        }

        public MessageApiModel(Message message)
        {
            if (message != null)
            {
                this.DeviceId = message.DeviceId;
                this.time = message.Time;
                this.Data = message.Data;
            }
        }
    }
}
