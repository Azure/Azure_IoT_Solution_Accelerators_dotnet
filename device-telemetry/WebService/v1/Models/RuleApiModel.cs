// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rule = Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Rule;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class RuleApiModel
    {
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";

        [JsonProperty(PropertyName = "ETag")]
        public string ETag { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "DateCreated")]
        public string DateCreated { get; set; } = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);

        [JsonProperty(PropertyName = "DateModified")]
        public string DateModified { get; set; } = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);

        [JsonProperty(PropertyName = "Enabled")]
        public bool Enabled { get; set; } = false;

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "GroupId")]
        public string GroupId { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Severity")]
        public string Severity { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Conditions")]
        public List<ConditionApiModel> Conditions { get; set; } = new List<ConditionApiModel>();

        // Possible values -["average", "instant"]
        [JsonProperty(PropertyName = "Calculation")]
        public string Calculation { get; set; } = string.Empty;

        // Possible values -["60000", "300000", "600000"] in milliseconds
        [JsonProperty(PropertyName = "TimePeriod")]
        public string TimePeriod { get; set; } = "0";

        [JsonProperty(PropertyName = "Actions")]
        public List<ActionApiModel> Actions { get; set; } = new List<ActionApiModel>();

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public IDictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", "Rule;" + Version.NUMBER },
            { "$uri", "/" + Version.PATH + "/rules/" + this.Id }
        };

        [JsonProperty(PropertyName = "Deleted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Deleted { get; set; }

        public RuleApiModel() { }

        public RuleApiModel(Rule rule, bool includeDeleted)
        {
            if (rule != null)
            {
                this.ETag = rule.ETag;
                this.Id = rule.Id;
                this.Name = rule.Name;
                this.DateCreated = rule.DateCreated;
                this.DateModified = rule.DateModified;
                this.Enabled = rule.Enabled;
                this.Description = rule.Description;
                this.GroupId = rule.GroupId;
                this.Severity = rule.Severity.ToString();
                this.Calculation = rule.Calculation.ToString();
                this.TimePeriod = rule.TimePeriod.ToString();
                if (includeDeleted)
                {
                    this.Deleted = rule.Deleted;
                }

                foreach (IActionItem action in rule.Actions)
                {
                    this.Actions.Add(new ActionApiModel(action));
                }

                foreach (Condition condition in rule.Conditions)
                {
                    this.Conditions.Add(new ConditionApiModel(condition));
                }
            }
        }

        public Rule ToServiceModel()
        {
            List<Condition> conditions = new List<Condition>();
            List<IActionItem> actions = new List<IActionItem>();

            foreach (ConditionApiModel condition in this.Conditions)
            {
                conditions.Add(condition.ToServiceModel());
            }

            foreach (ActionApiModel action in this.Actions)
            {
                actions.Add(action.ToServiceModel());
            }

            if (!Enum.TryParse<CalculationType>(this.Calculation, true, out CalculationType calculation))
            {
                throw new InvalidInputException($"The value of 'Calculation' - '{this.Calculation}' is not valid");
            }

            if (!Enum.TryParse<SeverityType>(this.Severity, true, out SeverityType severity))
            {
                throw new InvalidInputException($"The value of 'Severity' - '{this.Severity}' is not valid");
            }

            if (!long.TryParse(!string.IsNullOrEmpty(this.TimePeriod) ? this.TimePeriod : "0", out long timePeriod) || (calculation == CalculationType.Average && string.IsNullOrEmpty(this.TimePeriod)))
            {
                throw new InvalidInputException($"The value of 'TimePeriod' - '{this.TimePeriod}' is not valid");
            }

            bool deleted = false;
            if (this.Deleted.HasValue)
            {
                deleted = this.Deleted.Value;
            }

            return new Rule()
            {
                ETag = this.ETag,
                Id = this.Id,
                Name = this.Name,
                DateCreated = this.DateCreated,
                DateModified = this.DateModified,
                Enabled = this.Enabled,
                Description = this.Description,
                GroupId = this.GroupId,
                Severity = severity,
                Calculation = calculation,
                TimePeriod = timePeriod,
                Conditions = conditions,
                Actions = actions,
                Deleted = deleted
            };
        }
    }
}
