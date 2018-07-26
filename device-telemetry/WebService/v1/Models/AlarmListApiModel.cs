// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmListApiModel
    {
        [JsonProperty(PropertyName = "Items")]
        public List<AlarmApiModel> Items { get; set; }

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", $"Alarms;" + Version.NUMBER },
            { "$uri", "/" + Version.PATH + "/alarms" }
        };

        public AlarmListApiModel(List<Alarm> alarms)
        {
            this.Items = new List<AlarmApiModel>();
            if (alarms != null)
            {
                foreach (Alarm alarm in alarms)
                {
                    this.Items.Add(new AlarmApiModel(alarm));
                }
            }
        }
    }
}
