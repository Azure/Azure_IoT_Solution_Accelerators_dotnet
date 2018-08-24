// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models
{
    public class AlarmNotificationAsaModel
    {
        [JsonProperty(PropertyName = "created")]
        public string DateCreated { get; set; }

        [JsonProperty(PropertyName = "modified")]
        public string DateModified { get; set; }

        [JsonProperty(PropertyName = "rule.description")]
        public string Rule_description { get; set; }

        [JsonProperty(PropertyName = "rule.severity")]
        public string Rule_severity { get; set; }

        [JsonProperty(PropertyName = "rule.id")]
        public string Rule_id { get; set; }

        [JsonProperty(PropertyName = "rule.actions")]
        public IList<ActionAsaModel> Actions { get; set; }

        [JsonProperty(PropertyName = "device.id")]
        public string Device_id { get; set; }

        [JsonProperty(PropertyName = "device.msg.received")]
        public string Message_received { get; set; }

        public AlarmNotificationAsaModel() { }
    }
}
