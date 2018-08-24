// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.NotificationSystem
{
    public class LogicAppTest
    {
        private readonly Mock<ILogger> logMock;
        private readonly Mock<IHttpRequest> httpRequestMock;
        private readonly Mock<IHttpClient> httpClientMock;

        private LogicApp tempLogicApp;

        public LogicAppTest()
        {
            this.logMock = new Mock<ILogger>();
            this.httpClientMock = new Mock<IHttpClient>();
            this.httpRequestMock = new Mock<IHttpRequest>();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnInvalidRequestStatusCode_WhenInValidEndPointUrl()
        {
            // Setup
            tempLogicApp = new LogicApp(It.IsAny<string>(), It.IsAny<string>(),
                this.httpRequestMock.Object, this.httpClientMock.Object, this.logMock.Object);
            tempLogicApp.SetReceiver(new List<string>() { It.IsAny<string>() });
            this.httpClientMock.Setup(x => x.PostAsync(It.IsAny<IHttpRequest>())).Returns(
                Task.FromResult<IHttpResponse>(new HttpResponse(0, It.IsAny<string>(), It.IsAny<HttpResponseHeaders>())));

            // Act
            var logicAppRequestStatusCode = tempLogicApp.Execute().Result;

            Assert.Equal<HttpStatusCode>(0, logicAppRequestStatusCode);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ReturnOkStatusCode_When_ValidEndPointUrl()
        {
            // Setup
            tempLogicApp = new LogicApp(It.IsAny<string>(), It.IsAny<string>(),
                this.httpRequestMock.Object, this.httpClientMock.Object, this.logMock.Object);
            tempLogicApp.SetReceiver(new List<string>() { It.IsAny<string>() });
            this.httpClientMock.Setup(x => x.PostAsync(It.IsAny<IHttpRequest>())).Returns(
                Task.FromResult<IHttpResponse>(new HttpResponse(System.Net.HttpStatusCode.OK, It.IsAny<string>(), It.IsAny<HttpResponseHeaders>())));

            // Act
            var logicAppRequestStatusCode = tempLogicApp.Execute().Result;

            // Assert
            Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, logicAppRequestStatusCode);
        }

    }
}
