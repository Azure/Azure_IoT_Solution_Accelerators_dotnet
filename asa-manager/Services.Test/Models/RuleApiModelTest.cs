// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.Models
{
    public class RuleApiModelTest
    {
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void EmptyInstancesAreEqual()
        {
            // Arrange
            var x = new RuleApiModel();
            var y = new RuleApiModel();

            // Assert
            Assert.True(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void NonEmptyInstancesWithSameDataAreEqual()
        {
            // Arrange: rule without conditions
            var x = new RuleApiModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Enabled = true,
                Description = Guid.NewGuid().ToString(),
                GroupId = Guid.NewGuid().ToString(),
                Severity = Guid.NewGuid().ToString(),
                Conditions = new List<ConditionApiModel>(),
                Deleted = false
            };
            var y = Clone(x);

            // Assert
            Assert.True(x.Equals(y));

            // Arrange: rule with conditions
            x = new RuleApiModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Enabled = true,
                Description = Guid.NewGuid().ToString(),
                GroupId = Guid.NewGuid().ToString(),
                Severity = Guid.NewGuid().ToString(),
                Conditions = new List<ConditionApiModel>
                {
                    new ConditionApiModel { Field = "temp", Operator = ">=", Value = "75" },
                    new ConditionApiModel { Field = "hum", Operator = "gt", Value = "50" },
                }
            };
            y = Clone(x);

            // Assert
            Assert.True(x.Equals(y));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithDifferentDataAreDifferent()
        {
            // Arrange
            var x = new RuleApiModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Enabled = true,
                Description = Guid.NewGuid().ToString(),
                GroupId = Guid.NewGuid().ToString(),
                Severity = Guid.NewGuid().ToString(),
                Conditions = new List<ConditionApiModel>(),
                Deleted = false
            };
            var y1 = Clone(x);
            var y2 = Clone(x);
            var y3 = Clone(x);
            var y4 = Clone(x);
            var y5 = Clone(x);
            var y6 = Clone(x);

            y1.Id += "x";
            y2.Name += "x";
            y3.Enabled = !y3.Enabled;
            y4.GroupId += "x";
            y5.Severity += "x";
            y6.Deleted = !y6.Deleted;

            // Assert
            Assert.False(x.Equals(y1));
            Assert.False(x.Equals(y2));
            Assert.False(x.Equals(y3));
            Assert.False(x.Equals(y4));
            Assert.False(x.Equals(y5));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InstancesWithDifferentConditionsAreDifferent()
        {
            // Arrange: different number of conditions
            var x = new RuleApiModel
            {
                Conditions = new List<ConditionApiModel>()
            };
            var y = Clone(x);
            y.Conditions.Add(new ConditionApiModel());

            // Assert
            Assert.False(x.Equals(y));

            // Arrange: different field
            x.Conditions = new List<ConditionApiModel>
            {
                new ConditionApiModel { Field = "x", Operator = ">=", Value = "5" }
            };
            y = Clone(x);
            y.Conditions[0].Field = "y";

            // Assert
            Assert.False(x.Equals(y));

            // Arrange: different operator
            y = Clone(x);
            y.Conditions[0].Operator = "<";

            // Assert
            Assert.False(x.Equals(y));

            // Arrange: different value
            y = Clone(x);
            y.Conditions[0].Value = "123";

            // Assert
            Assert.False(x.Equals(y));
        }

        private static T Clone<T>(T o)
        {
            return JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(o));
        }
    }
}
