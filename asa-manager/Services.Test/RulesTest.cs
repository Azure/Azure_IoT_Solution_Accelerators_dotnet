// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.IoTSolutions.AsaManager.Services;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Http;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Moq;
using Services.Test.helpers;
using Xunit;
using Xunit.Abstractions;

namespace Services.Test
{
    public class RulesTest
    {
        private readonly Rules target;
        private readonly Mock<IHttpClient> httpClient;
        private readonly Mock<ILogger> log;

        public RulesTest(ITestOutputHelper log)
        {
            var config = new Mock<IServicesConfig>();
            config.Setup(x => x.RulesWebServiceTimeout).Returns(1000);
            config.Setup(x => x.RulesWebServiceUrl).Returns("http://127.0.0.1/v1/rules");

            this.httpClient = new Mock<IHttpClient>();
            this.log = new Mock<ILogger>();
            this.target = new Rules(config.Object, this.httpClient.Object, this.log.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItLoadsRulesAndReturnOnlyActiveOnes()
        {
            // Arrange
            var response = new HttpResponse(
                HttpStatusCode.OK,
                "{'Items': [" +
                "{'Id':'C4','Enabled':true}," +
                "{'Id':'Z2','Enabled':false}," +
                "{'Id':'B6','Enabled':false}," +
                "{'Id':'B7','Enabled':false}," +
                "{'Id':'B1','Enabled':false}," +
                "{'Id':'B4','Enabled':true}," +
                "{'Id':'44','Enabled':false}," +
                "{'Id':'54','Enabled':true,'Deleted':true}," +
                "{'Id':'74','Enabled':false,'Deleted':true}" +
                "]}",
                null);
            this.httpClient.Setup(
                    x => x.GetAsync(It.IsAny<HttpRequest>()))
                .ReturnsAsync(response);

            // Act
            var rules = this.target.GetActiveRulesSortedByIdAsync().Result;

            // Assert
            Assert.Equal(2, rules.Count);
            Assert.Equal("B4", rules[0].Id);
            Assert.Equal("C4", rules[1].Id);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItReturnRulesSortedById()
        {
            // Arrange
            var response = new HttpResponse(
                HttpStatusCode.OK,
                "{'Items': [" +
                "{'Id':'C4','Enabled':true}," +
                "{'Id':'Z2','Enabled':true}," +
                "{'Id':'B6','Enabled':true}," +
                "{'Id':'B4','Enabled':true}" +
                "]}",
                null);
            this.httpClient.Setup(
                    x => x.GetAsync(It.IsAny<HttpRequest>()))
                .ReturnsAsync(response);

            // Act
            var rules = this.target.GetActiveRulesSortedByIdAsync().Result;

            // Assert
            Assert.Equal(4, rules.Count);
            Assert.Equal("B4", rules[0].Id);
            Assert.Equal("B6", rules[1].Id);
            Assert.Equal("C4", rules[2].Id);
            Assert.Equal("Z2", rules[3].Id);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItComparesListOfRules()
        {
            // Arrange: two empty lists
            var list1 = new List<RuleApiModel>();
            var list2 = new List<RuleApiModel>();

            // Assert
            Assert.True(this.target.RulesAreEquivalent(list1, list2));

            // Arrange: two equivalent non empty lists
            list1 = new List<RuleApiModel> { new RuleApiModel { Id = "1ab" } };
            list2 = new List<RuleApiModel> { new RuleApiModel { Id = "1ab" } };

            // Assert
            Assert.True(this.target.RulesAreEquivalent(list1, list2));

            // Arrange: two lists of different size
            list1 = new List<RuleApiModel> { new RuleApiModel { Id = "2bc" }, new RuleApiModel { Id = "1ab" } };
            list2 = new List<RuleApiModel> { new RuleApiModel { Id = "2bc" } };

            // Assert
            Assert.False(this.target.RulesAreEquivalent(list1, list2));

            // Arrange: two different lists
            list1 = new List<RuleApiModel> { new RuleApiModel { Id = "1ab" }, new RuleApiModel { Id = "1bc" } };
            list2 = new List<RuleApiModel> { new RuleApiModel { Id = "2bc" }, new RuleApiModel { Id = "3ab" } };

            // Assert
            Assert.False(this.target.RulesAreEquivalent(list1, list2));
        }
    }
}
