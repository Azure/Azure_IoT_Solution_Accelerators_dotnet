// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using AsaConfigAgent.Test.helpers;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent.Models;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace TelemetryRulesAgent.Test.Models
{
    public class AsaRefDataRuleTest
    {
        // Value used by the Rules web service to indicate that
        // a rule doesn't use aggregation and has no time window.
        // See https://github.com/Azure/device-telemetry-dotnet/blob/master/Services/Models/Rule.cs
        private const string SOURCE_NO_AGGREGATION = "instant";
        private const string SOURCE_AVG_AGGREGATOR = "avg";
        private const string SOURCE_MIN_AGGREGATOR = "min";
        private const string SOURCE_MAX_AGGREGATOR = "max";
        private const string SOURCE_COUNT_AGGREGATOR = "count";
        private const long MILLISECONDS_PER_SECOND = 1000;
        private const long SOURCE_1MIN_AGGREGATION = 60 * MILLISECONDS_PER_SECOND;
        private const long SOURCE_5MINS_AGGREGATION = 300 * MILLISECONDS_PER_SECOND;
        private const long SOURCE_10MINS_AGGREGATION = 600 * MILLISECONDS_PER_SECOND;
        private const long SOURCE_20MINS_AGGREGATION = 1200 * MILLISECONDS_PER_SECOND;
        private const long SOURCE_30MINS_AGGREGATION = 1800 * MILLISECONDS_PER_SECOND;
        private const long SOURCE_1HOUR_AGGREGATION = 3600 * MILLISECONDS_PER_SECOND;

        // Used to tell ASA TSQL which aggregation to use
        private const string ASA_AGGREGATION_NONE = "instant";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_1MIN = "tumblingwindow1minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_5MINS = "tumblingwindow5minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_10MINS = "tumblingwindow10minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_20MINS = "tumblingwindow20minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_30MINS = "tumblingwindow30minutes";
        private const string ASA_AGGREGATION_WINDOW_TUMBLING_1HOUR = "tumblingwindow1hours";
        private const string ASA_JS_AVG_FIELD = ".avg";
        private const string ASA_JS_MIN_FIELD = ".min";
        private const string ASA_JS_MAX_FIELD = ".max";
        private const string ASA_JS_COUNT_FIELD = ".count";

        private ITestOutputHelper log;

        public AsaRefDataRuleTest(ITestOutputHelper log)
        {
            this.log = log;
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItDoesntHaveFieldsByDefault()
        {
            // Act
            var target1 = new AsaRefDataRule();
            var target2 = new AsaRefDataRule(new RuleApiModel());

            // Assert
            Assert.Equal(0, target1.Fields.Count);
            Assert.Equal(0, target2.Fields.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItCanBeSerializedToJson()
        {
            // Arrange
            var rule = new RuleApiModel { Enabled = true };

            // Act
            var target = new AsaRefDataRule(rule);
            var json = JsonConvert.SerializeObject(target);
            this.log.WriteLine("JSON: " + json);

            // Assert
            var expectedJSON = JsonConvert.SerializeObject(new
            {
                Id = (string) null,
                Name = (string) null,
                Description = (string) null,
                GroupId = (string) null,
                Severity = (string) null,
                AggregationWindow = (string) null,
                Fields = new string[] { },
                __rulefilterjs = "return true;"
            });
            Assert.Equal(expectedJSON, json);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void JSONSerializationOfOneRuleWithoutConditionsWithoutTimeWindowIsCorrect()
        {
            // Arrange
            var rule = new RuleApiModel
            {
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                GroupId = Guid.NewGuid().ToString(),
                Severity = Guid.NewGuid().ToString(),
                Calculation = SOURCE_NO_AGGREGATION
            };

            // Act
            var target = new AsaRefDataRule(rule);
            var json = JsonConvert.SerializeObject(target);
            this.log.WriteLine("JSON: " + json);

            // Assert
            var expectedJSON = JsonConvert.SerializeObject(new
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                GroupId = rule.GroupId,
                Severity = rule.Severity,
                AggregationWindow = ASA_AGGREGATION_NONE,
                Fields = new string[] { },
                __rulefilterjs = "return true;"
            });
            Assert.Equal(expectedJSON, json);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(SOURCE_1MIN_AGGREGATION, ASA_AGGREGATION_WINDOW_TUMBLING_1MIN)]
        [InlineData(SOURCE_5MINS_AGGREGATION, ASA_AGGREGATION_WINDOW_TUMBLING_5MINS)]
        [InlineData(SOURCE_10MINS_AGGREGATION, ASA_AGGREGATION_WINDOW_TUMBLING_10MINS)]
        [InlineData(SOURCE_20MINS_AGGREGATION, ASA_AGGREGATION_WINDOW_TUMBLING_20MINS)]
        [InlineData(SOURCE_30MINS_AGGREGATION, ASA_AGGREGATION_WINDOW_TUMBLING_30MINS)]
        [InlineData(SOURCE_1HOUR_AGGREGATION, ASA_AGGREGATION_WINDOW_TUMBLING_1HOUR)]
        public void JSONSerializationOfOneRuleWithoutConditionsWithTimeWindowIsCorrect(long sourceAggr, string asaAggr)
        {
            // Arrange
            var rule = new RuleApiModel
            {
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                GroupId = Guid.NewGuid().ToString(),
                Severity = Guid.NewGuid().ToString(),
                Calculation = SOURCE_AVG_AGGREGATOR,
                TimePeriod = sourceAggr
            };

            // Act
            var target = new AsaRefDataRule(rule);
            var json = JsonConvert.SerializeObject(target);
            this.log.WriteLine("JSON: " + json);

            // Assert
            var expectedJSON = JsonConvert.SerializeObject(new
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                GroupId = rule.GroupId,
                Severity = rule.Severity,
                AggregationWindow = asaAggr,
                Fields = new string[] { },
                __rulefilterjs = "return true;"
            });
            Assert.Equal(expectedJSON, json);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("GreaterThan", ">")]
        [InlineData("greaterthan", ">")]
        [InlineData(">", ">")]
        [InlineData("GreaterThanOrEqual", ">=")]
        [InlineData("greaterthanorequal", ">=")]
        [InlineData(">=", ">=")]
        [InlineData("LessThan", "<")]
        [InlineData("lessthan", "<")]
        [InlineData("<", "<")]
        [InlineData("LessThanOrEqual", "<=")]
        [InlineData("lessthanorequal", "<=")]
        [InlineData("<=", "<=")]
        [InlineData("Equals", "=")]
        [InlineData("equals", "=")]
        [InlineData("=", "=")]
        [InlineData("==", "=")]
        public void JSONSerializationOfOneRuleWithConditionsWithoutTimeWindowIsCorrect(string sourceOperator, string jsOperator)
        {
            // Arrange
            var rule = new RuleApiModel
            {
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                GroupId = Guid.NewGuid().ToString(),
                Severity = Guid.NewGuid().ToString(),
                Calculation = SOURCE_NO_AGGREGATION,
                Conditions = new List<ConditionApiModel>
                {
                    new ConditionApiModel
                    {
                        Field = Guid.NewGuid().ToString("N"),
                        Operator = sourceOperator,
                        Value = (new Random().Next(-10000, 10000) / 1.1).ToString()
                    },
                    new ConditionApiModel
                    {
                        Field = Guid.NewGuid().ToString("N"),
                        Operator = sourceOperator,
                        Value = new Random().Next(-10000, 10000).ToString()
                    },
                    new ConditionApiModel
                    {
                        Field = Guid.NewGuid().ToString("N"),
                        Operator = sourceOperator,
                        Value = (new Random().Next(-10000, 10000) / 1.3).ToString()
                    },
                }
            };

            // Act
            var target = new AsaRefDataRule(rule);
            var json = JsonConvert.SerializeObject(target);
            this.log.WriteLine("JSON: " + json);

            // Assert
            var cond1 = $"record.__aggregates.{rule.Conditions[0].Field} {jsOperator} {rule.Conditions[0].Value}";
            var cond2 = $"record.__aggregates.{rule.Conditions[1].Field} {jsOperator} {rule.Conditions[1].Value}";
            var cond3 = $"record.__aggregates.{rule.Conditions[2].Field} {jsOperator} {rule.Conditions[2].Value}";
            var expectedJSON = JsonConvert.SerializeObject(new
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                GroupId = rule.GroupId,
                Severity = rule.Severity,
                AggregationWindow = ASA_AGGREGATION_NONE,
                Fields = rule.Conditions.Select(x => x.Field),
                __rulefilterjs = $"return ({cond1} && {cond2} && {cond3}) ? true : false;"
            });
            Assert.Equal(expectedJSON, json);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("GreaterThan", ">")]
        [InlineData("greaterthan", ">")]
        [InlineData(">", ">")]
        [InlineData("GreaterThanOrEqual", ">=")]
        [InlineData("greaterthanorequal", ">=")]
        [InlineData(">=", ">=")]
        [InlineData("LessThan", "<")]
        [InlineData("lessthan", "<")]
        [InlineData("<", "<")]
        [InlineData("LessThanOrEqual", "<=")]
        [InlineData("lessthanorequal", "<=")]
        [InlineData("<=", "<=")]
        [InlineData("Equals", "=")]
        [InlineData("equals", "=")]
        [InlineData("=", "=")]
        [InlineData("==", "=")]
        public void JSONSerializationOfOneRuleWithConditionsWithTimeWindowIsCorrect(string sourceOperator, string jsOperator)
        {
            // Arrange
            var rule = new RuleApiModel
            {
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                GroupId = Guid.NewGuid().ToString(),
                Severity = Guid.NewGuid().ToString(),
                Calculation = SOURCE_AVG_AGGREGATOR,
                TimePeriod = SOURCE_5MINS_AGGREGATION,
                Conditions = new List<ConditionApiModel>
                {
                    new ConditionApiModel
                    {
                        Field = Guid.NewGuid().ToString("N"),
                        Operator = sourceOperator,
                        Value = (new Random().Next(-10000, 10000) / 1.1).ToString()
                    },
                    new ConditionApiModel
                    {
                        Field = Guid.NewGuid().ToString("N"),
                        Operator = sourceOperator,
                        Value = new Random().Next(-10000, 10000).ToString()
                    },
                    new ConditionApiModel
                    {
                        Field = Guid.NewGuid().ToString("N"),
                        Operator = sourceOperator,
                        Value = (new Random().Next(-10000, 10000) / 1.3).ToString()
                    },
                }
            };

            // Act
            var target = new AsaRefDataRule(rule);
            var json = JsonConvert.SerializeObject(target);
            this.log.WriteLine("JSON: " + json);

            // Assert
            var cond1 = $"record.__aggregates.{rule.Conditions[0].Field}{ASA_JS_AVG_FIELD} {jsOperator} {rule.Conditions[0].Value}";
            var cond2 = $"record.__aggregates.{rule.Conditions[1].Field}{ASA_JS_AVG_FIELD} {jsOperator} {rule.Conditions[1].Value}";
            var cond3 = $"record.__aggregates.{rule.Conditions[2].Field}{ASA_JS_AVG_FIELD} {jsOperator} {rule.Conditions[2].Value}";
            var expectedJSON = JsonConvert.SerializeObject(new
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                GroupId = rule.GroupId,
                Severity = rule.Severity,
                AggregationWindow = ASA_AGGREGATION_WINDOW_TUMBLING_5MINS,
                Fields = rule.Conditions.Select(x => x.Field),
                __rulefilterjs = $"return ({cond1} && {cond2} && {cond3}) ? true : false;"
            });
            Assert.Equal(expectedJSON, json);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(SOURCE_AVG_AGGREGATOR, ASA_JS_AVG_FIELD)]
        [InlineData(SOURCE_MIN_AGGREGATOR, ASA_JS_MIN_FIELD)]
        [InlineData(SOURCE_MAX_AGGREGATOR, ASA_JS_MAX_FIELD)]
        [InlineData(SOURCE_COUNT_AGGREGATOR, ASA_JS_COUNT_FIELD)]
        public void JSONSerializationOfRulesWithMinMaxAvgEtcAggregationIsCorrect(string aggregator, string jsField)
        {
            // Arrange
            var rule = new RuleApiModel
            {
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                GroupId = Guid.NewGuid().ToString(),
                Severity = Guid.NewGuid().ToString(),
                Calculation = aggregator,
                TimePeriod = SOURCE_5MINS_AGGREGATION,
                Conditions = new List<ConditionApiModel>
                {
                    new ConditionApiModel { Field = "foo", Operator = ">", Value = "123" }
                }
            };

            // Act
            var target = new AsaRefDataRule(rule);
            var json = JsonConvert.SerializeObject(target);
            this.log.WriteLine("JSON: " + json);

            // Assert
            var cond1 = $"record.__aggregates.{rule.Conditions[0].Field}{jsField} > {rule.Conditions[0].Value}";
            var expectedJSON = JsonConvert.SerializeObject(new
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                GroupId = rule.GroupId,
                Severity = rule.Severity,
                AggregationWindow = ASA_AGGREGATION_WINDOW_TUMBLING_5MINS,
                Fields = rule.Conditions.Select(x => x.Field),
                __rulefilterjs = $"return ({cond1}) ? true : false;"
            });
            Assert.Equal(expectedJSON, json);
        }
    }
}
