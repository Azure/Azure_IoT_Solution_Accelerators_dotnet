// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmStatusApiModel
    {
        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }

        public AlarmStatusApiModel()
        {
            this.Status = null;
        }

        public AlarmStatusApiModel(string status)
        {
            this.Status = status;
        }
    }
}
