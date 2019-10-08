// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.ActionsAgent.Test
{
    public class ActionManagerTest
    {
        private readonly ActionManager actionManager;
        private readonly Mock<IHttpClient> httpClientMock;

        public ActionManagerTest()
        {
            Mock<ILogger> loggerMock = new Mock<ILogger>();
            this.httpClientMock = new Mock<IHttpClient>();

            char pathSeparator = Path.DirectorySeparatorChar;
            IServicesConfig servicesConfig = new ServicesConfig
            {
                LogicAppEndpointUrl = "https://azure.com",
                SolutionUrl = "test",
                TemplateFolder = $".{pathSeparator}data{pathSeparator}"
            };
            this.actionManager = new ActionManager(loggerMock.Object, servicesConfig, this.httpClientMock.Object);
        }


        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task EmailAction_CausesPostToLogicApp()
        {
            // Arrange
            JArray emailArray = new JArray(new object[] { "sampleEmail@gmail.com" });
            Dictionary<string, object> actionParameters = new Dictionary<string, object>
            {
                { "Recipients", emailArray },
                { "Notes", "Test Note" },
                { "Subject", "Test Subject" }
            };
            EmailAction testAction = new EmailAction(actionParameters);
            AsaAlarmApiModel alarm = new AsaAlarmApiModel
            {
                DateCreated = 1539035437937,
                DateModified = 1539035437937,
                DeviceId = "Test Device Id",
                MessageReceived = 1539035437937,
                RuleDescription = "Test Rule description",
                RuleId = "TestRuleId",
                RuleSeverity = "Warning",
                Actions = new List<IAction> { testAction }
            };

            var response = new HttpResponse(HttpStatusCode.OK, "", null);

            this.httpClientMock.Setup(x => x.PostAsync(It.IsAny<IHttpRequest>())).ReturnsAsync(response);
            List<AsaAlarmApiModel> alarmList = new List<AsaAlarmApiModel> { alarm };

            // Act
            await this.actionManager.ExecuteAlarmActions(alarmList);

            // Assert
            this.httpClientMock.Verify(x => x.PostAsync(It.IsAny<IHttpRequest>()));
        }
    }
}
