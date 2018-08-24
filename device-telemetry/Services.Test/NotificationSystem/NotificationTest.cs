// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.NotificationSystem
{
    public class NotificationTest
    {
        Mock<IImplementationWrapper> implementationWrapperMock;
        Mock<INotificationImplementation> implementationMock;
        Mock<ILogger> logMock;

        public NotificationTest()
        {
            this.implementationWrapperMock = new Mock<IImplementationWrapper>();
            this.implementationMock = new Mock<INotificationImplementation>();
            this.logMock = new Mock<ILogger>();
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(2, 2)]
        [InlineData(0, 0)]
        public void Should_CallExecuteMethodEqualToTheNumberOfCalls_To_TheNumberOfActionItemsInAlert(int numOfActionItems, int numOfCalls)
        {
            // Arrange
            var tempNotification = new Notification(this.implementationWrapperMock.Object, this.logMock.Object)
            {
                alarm = this.getSampleAlarmWithNActions(numOfActionItems)
            };
            this.implementationWrapperMock.Setup(a => a.GetImplementationType(It.IsAny<EmailImplementationTypes>())).Returns(this.implementationMock.Object);
            this.implementationMock.Setup(a => a.Execute()).Returns(Task.FromResult<HttpStatusCode>(HttpStatusCode.OK));

            // Act
            tempNotification.execute().Wait();

            // Assert
            this.implementationMock.Verify(a => a.Execute(), Times.Exactly(numOfCalls));
            this.implementationMock.Verify(a => a.SetMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(numOfCalls));
            this.implementationMock.Verify(a => a.SetReceiver(It.IsAny<List<string>>()), Times.Exactly(numOfCalls));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_NotCallExecuteMethod_When_AlertHasNoActions()
        {
            // Arrange
            var tempNotification = new Notification(this.implementationWrapperMock.Object, this.logMock.Object)
            {
                alarm = new AlarmNotificationAsaModel()
                {
                    Actions = new List<ActionAsaModel>()
                }
            };

            // Act
            tempNotification.execute().Wait();

            // Assert
            this.implementationMock.Verify(a => a.Execute(), Times.Never);
            this.implementationMock.Verify(a => a.SetMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            this.implementationMock.Verify(a => a.SetReceiver(It.IsAny<List<string>>()), Times.Never);
        }

        private AlarmNotificationAsaModel getSampleAlarmWithNActions(int n)
        {
            return new AlarmNotificationAsaModel()
            {
                Rule_id = "12345",
                Rule_description = "Sample test description",
                Actions = Enumerable.Repeat(this.getSampleAction(), n).ToList<ActionAsaModel>()
            };
        }

        private ActionAsaModel getSampleAction()
        {
            return new ActionAsaModel()
            {
                ActionType = "Email",
                Parameters = new Dictionary<string, object>()
                {
                    {"Template", "Hey this is a test email" },
                    {"Subject", "Test Subject" },
                    {"Email", new Newtonsoft.Json.Linq.JArray() {"Email1@gmail.com", "Email2@gmail.com"} }
                }
            };
        }
    }
}
