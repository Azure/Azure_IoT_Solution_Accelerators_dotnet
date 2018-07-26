// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class RuleListApiModel
    {
        private List<RuleApiModel> items;

        [JsonProperty(PropertyName = "Items")]
        public List<RuleApiModel> Items
        {
            get { return this.items; }
        }

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public IDictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", "RuleList;" + Version.NUMBER },
            { "$uri", "/" + Version.PATH + "/rules" },
        };

        public RuleListApiModel(List<Rule> rules, bool includeDeleted)
        {
            this.items = new List<RuleApiModel>();
            if (rules != null)
            {
                foreach (Rule rule in rules)
                {
                    this.items.Add(new RuleApiModel(rule, includeDeleted));
                }
            }
        }
    }
}
