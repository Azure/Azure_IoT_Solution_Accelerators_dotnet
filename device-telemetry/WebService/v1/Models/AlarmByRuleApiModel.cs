// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmByRuleApiModel
    {
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";
        private DateTimeOffset created;
        private int count;

        [JsonProperty(PropertyName = "Count")]
        public int Count
        {
            get { return this.count; }
            set { this.count = value; }
        }

        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "Created")]
        public string Created
        {
            get { return this.created.ToString(DATE_FORMAT); }
            set { this.created = DateTimeOffset.Parse(value); }
        }

        [JsonProperty(PropertyName = "Rule")]
        public AlarmRuleApiModel Rule { get; set; }

        public AlarmByRuleApiModel(
            int count,
            string status,
            DateTimeOffset created,
            AlarmRuleApiModel rule)
        {
            this.count = count;
            this.Status = status;
            this.created = created;
            this.Rule = rule;

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"AlarmByRule;" + Version.NUMBER },
                { "$uri", "/" + Version.PATH + "/alarmsbyrule/" + this.Rule.Id }
            };
        }

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public Dictionary<string, string> Metadata { get; set; }

        public AlarmByRuleApiModel(
            int count,
            string status,
            DateTimeOffset created,
            Alarm alarm)
        {
            this.Count = count;
            this.Status = status;
            this.created = created;
            this.Rule = new AlarmRuleApiModel(
                alarm.RuleId,
                alarm.RuleSeverity,
                alarm.RuleDescription);

            this.Metadata = new Dictionary<string, string>
            {
                { "$type", $"AlarmByRule;" + Version.NUMBER },
                { "$uri", "/" + Version.PATH + "/alarmsbyrule/" + this.Rule.Id }
            };
        }
    }
}
