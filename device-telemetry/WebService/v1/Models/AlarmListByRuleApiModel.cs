// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmListByRuleApiModel : AlarmListApiModel
    {
        public AlarmListByRuleApiModel(List<Alarm> alarms) : base(alarms)
        {
        }

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public new Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", $"AlarmsByRule;" + Version.NUMBER },
            { "$uri", "/" + Version.PATH + "/alarmsbyrule" }
        };
    }
}
