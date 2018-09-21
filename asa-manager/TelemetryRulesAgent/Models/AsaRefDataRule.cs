// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent.Models
{
    // Note: all the constants below are meant to be not case sensitive
    public class AsaRefDataRule
    {
        // Value used by the Rules web service to indicate that
        // a rule doesn't use aggregation and has no time window.
        // See https://github.com/Azure/device-telemetry-dotnet/blob/master/Services/Models/Rule.cs
        private const string SOURCE_NO_AGGREGATION = "instant";

        // Used to tell ASA TSQL which aggregation to use
        private const string ASA_AGGREGATION_NONE = "instant";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_1MIN = "tumblingwindow1minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_5MINS = "tumblingwindow5minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_10MINS = "tumblingwindow10minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_20MINS = "tumblingwindow20minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_30MINS = "tumblingwindow30minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_1HOUR = "tumblingwindow1hours";

        // Map from values used in the Device Telemetry web service
        // to the corresponding ASA constant. Some extra values added
        // in case we want to add more options.
        // See https://github.com/Azure/device-telemetry-dotnet/blob/master/Services/Models/Rule.cs
        private static readonly Dictionary<long, string> timePeriodMap =
            new Dictionary<long, string>
            {
                {   60000, ASA_AGGREGATION_WINDOW_TUMBLING_1MIN },
                {  300000, ASA_AGGREGATION_WINDOW_TUMBLING_5MINS },
                {  600000, ASA_AGGREGATION_WINDOW_TUMBLING_10MINS },
                { 1200000, ASA_AGGREGATION_WINDOW_TUMBLING_20MINS },
                { 1800000, ASA_AGGREGATION_WINDOW_TUMBLING_30MINS },
                { 3600000, ASA_AGGREGATION_WINDOW_TUMBLING_1HOUR },
            };

        private const string ASA_INSTANT_VALUE = "";
        private const string ASA_AVG_VALUE = ".avg";
        private const string ASA_MIN_VALUE = ".min";
        private const string ASA_MAX_VALUE = ".max";
        private const string ASA_COUNT_VALUE = ".count";

        // Map from values used in the Device Telemetry web service to the
        // corresponding Javascript field. Some extra values added
        // in case we want to add more options.
        // See https://github.com/Azure/device-telemetry-dotnet/blob/master/Services/Models/Rule.cs
        private static readonly Dictionary<string, string> jsFieldsMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { SOURCE_NO_AGGREGATION, ASA_INSTANT_VALUE },
                { "avg", ASA_AVG_VALUE },
                { "average", ASA_AVG_VALUE },
                { "min", ASA_MIN_VALUE },
                { "minimum", ASA_MIN_VALUE },
                { "max", ASA_MAX_VALUE },
                { "maximum", ASA_MAX_VALUE },
                { "count", ASA_COUNT_VALUE },
            };

        // Map from values used in the Device Telemetry web service to the
        // corresponding Javascript symbols.
        // For flexibility, the keys are case insensitive, and also symbol-to-symbol mapping is supported.
        // See also https://github.com/Azure/device-telemetry-dotnet/blob/master/Services/Models/Condition.cs
        private static readonly Dictionary<string, string> operatorsMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "GreaterThan", ">" },
                { ">", ">" },
                { "GreaterThanOrEqual", ">=" },
                { ">=", ">=" },
                { "LessThan", "<" },
                { "<", "<" },
                { "LessThanOrEqual", "<=" },
                { "<=", "<=" },
                { "Equals", "=" },
                { "=", "=" },
                { "==", "=" },
            };

        // Internal data structure needed to serialize the model to JSON
        private readonly List<Condition> conditions;

        private struct Condition
        {
            internal string Calculation { get; set; }
            internal string Field { get; set; }
            internal string Operator { get; set; }
            internal string Value { get; set; }
        }

        private struct ActionItem
        {
            internal string ActionType { get; set; }
            internal IDictionary<string, object> Parameters { get; set; }
        }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("GroupId")]
        public string GroupId { get; set; }

        [JsonProperty("Severity")]
        public string Severity { get; set; }

        [JsonProperty("AggregationWindow")]
        public string AggregationWindow { get; set; }

        [JsonProperty("Fields")]
        public List<string> Fields { get; set; }

        [JsonProperty("Actions")]
        public List<IActionApiModel> Actions { get; set; }

        [JsonProperty("__rulefilterjs")]
        public string RuleFilterJs => this.ConditionsToJavascript();

        public AsaRefDataRule()
        {
            this.conditions = new List<Condition>();
            this.Fields = new List<string>();
            this.Actions = new List<IActionApiModel>();
        }

        public AsaRefDataRule(RuleApiModel rule) : this()
        {
            if (!rule.Enabled || rule.Deleted) return;

            this.Id = rule.Id;
            this.Name = rule.Name;
            this.Description = rule.Description;
            this.GroupId = rule.GroupId;
            this.Severity = rule.Severity;
            this.AggregationWindow = GetAggregationWindowValue(rule.Calculation, rule.TimePeriod);

            this.Fields = new List<string>();
            this.conditions = new List<Condition>();
            foreach (var c in rule.Conditions)
            {
                var condition = new Condition
                {
                    Calculation = rule.Calculation,
                    Field = c.Field,
                    Operator = c.Operator,
                    Value = c.Value
                };
                this.conditions.Add(condition);
                this.Fields.Add(c.Field);
            }
            this.Actions = rule.Actions;
        }

        private static string GetAggregationWindowValue(string calculation, long timePeriod)
        {
            if (string.IsNullOrEmpty(calculation))
            {
                return null;
            }

            if (calculation.ToLowerInvariant() == SOURCE_NO_AGGREGATION)
            {
                return ASA_AGGREGATION_NONE;
            }

            // Do not remove. This would be a bug, to be detected at development time.
            if (!timePeriodMap.ContainsKey(timePeriod))
            {
                throw new ApplicationException("Unknown time period: " + timePeriod);
            }

            return timePeriodMap[timePeriod];
        }

        private string ConditionsToJavascript()
        {
            if (this.conditions.Count == 0)
            {
                // A rule without conditions will always match
                return "return true;";
            }

            var result = string.Empty;
            foreach (var c in this.conditions)
            {
                // Concatenate conditions with AND
                if (!string.IsNullOrEmpty(result)) result += " && ";

                result += $"record.__aggregates." + GetFieldName(c.Field, c.Calculation);
                result += " " + GetJsOperator(c.Operator) + " ";
                result += c.Value;
            }

            return $"return ({result}) ? true : false;";
        }

        private static string GetFieldName(string field, string calculation)
        {
            // Do not remove. This would be a bug, to be detected at development time.
            if (!jsFieldsMap.ContainsKey(calculation))
            {
                throw new ApplicationException("Unknown calculation: " + calculation);
            }

            return field + jsFieldsMap[calculation];
        }

        private static string GetJsOperator(string op)
        {
            if (operatorsMap.ContainsKey(op)) return operatorsMap[op];

            // This is an overall bug in the solution, to be detected at development time
            throw new ApplicationException("Unknown operator: " + op);
        }
    }
}
