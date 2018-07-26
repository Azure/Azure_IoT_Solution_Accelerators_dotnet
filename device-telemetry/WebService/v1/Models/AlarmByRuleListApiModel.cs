// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmByRuleListApiModel
    {

        private readonly List<AlarmByRuleApiModel> items;

        [JsonProperty(PropertyName = "Items")]
        public List<AlarmByRuleApiModel> Items
        {
            get { return this.items; }

            private set { }
        }

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", $"AlarmByRuleList;" + Version.NUMBER },
            { "$uri", "/" + Version.PATH + "/alarmsbyrule" }
        };

        public AlarmByRuleListApiModel(List<AlarmCountByRule> alarmCountByRuleList)
        {
            this.items = new List<AlarmByRuleApiModel>();
            if (alarmCountByRuleList != null)
            {
                foreach (var alarm in alarmCountByRuleList)
                {
                    this.items.Add(new AlarmByRuleApiModel(
                        alarm.Count,
                        alarm.Status,
                        alarm.MessageTime,
                        new AlarmRuleApiModel(
                            alarm.Rule.Id,
                            alarm.Rule.Severity.ToString(),
                            alarm.Rule.Description)));
                }
            }
        }
    }
}
