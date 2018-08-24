// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.NotificationSystem
{
    /*
     * Event Data is an array of bytes. ProcessEventsAsync method takes in an Enumerable of EventData.
     * So, tests for ProcessEventsAsync method are designed as follows:
     * 
     * 1. When one event data has N json strings, check if number of calls to execute method same as number of Json Strings.
     * 2. When N event data, each with 1 json string, check if number of calls to execute method same as number of Event Data.
     * 3. When empty event data, should not call execute method.
     * 4. When N event data has M json strings, check if number of calls to execute method same as N*M number of Json strings. 
     * 
    */
    public class NotificationEventProcessorTest
    {
        private readonly Mock<INotification> notificationMock;
        private readonly Mock<ILogger> logMock;
        private readonly Mock<IServicesConfig> servicesConfigMock;

        private IEventProcessor notificationEventProcessor;

        public NotificationEventProcessorTest()
        {
            this.notificationMock = new Mock<INotification>();
            this.servicesConfigMock = new Mock<IServicesConfig>();
            this.logMock = new Mock<ILogger>();

            this.notificationEventProcessor = new NotificationEventProcessor(this.logMock.Object, this.servicesConfigMock.Object, this.notificationMock.Object);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(2, 2)]
        [InlineData(0, 0)]
        public void Should_CallExecuteForNTimesEqualToNumberOfJsonTokenInEventData_When_NJsonObjectsInOneEventData(int numJson, int numCalls)
        {
            // Setup
            this.notificationMock.Setup(a => a.execute()).Returns(Task.CompletedTask);
            var tempEventData = new EventData(getSamplePayLoadDataWithNalerts(numJson));
            this.notificationMock.Setup(a => a.alarm).Returns(this.getSampleAction());

            // Act
            this.notificationEventProcessor.ProcessEventsAsync(It.IsAny<PartitionContext>(), new EventData[] { tempEventData });

            // Assert
            this.notificationMock.Verify(e => e.execute(), Times.Exactly(numCalls));
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(2, 2)]
        [InlineData(0, 0)]
        public void Should_CallExecuteForNTimesEqualToNumberOfEventDataInMessages_When_EachEventDataHasOneJsonObject(int numEventData, int numCalls)
        {
            // Setup
            this.notificationMock.Setup(a => a.execute()).Returns(Task.CompletedTask);
            var tempEventData = new EventData(getSamplePayLoadDataWithNalerts(1));
            this.notificationMock.Setup(a => a.alarm).Returns(this.getSampleAction());

            // Act
            this.notificationEventProcessor.ProcessEventsAsync(It.IsAny<PartitionContext>(), Enumerable.Repeat<EventData>(tempEventData, numEventData));

            // Assert
            this.notificationMock.Verify(e => e.execute(), Times.Exactly(numCalls));
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(2, 2)]
        [InlineData(0, 0)]
        public void Should_ReturnAListOfProperJsonStrings_When_ValidInputJsonStringWithNnumberOfJsonTokens(int numJson, int numReturnJsonString)
        {
            // Setup
            var tempJson = getSampleJsonRepeatedNTimes(numJson);
            var tempNotificationEventProcessor = new NotificationEventProcessor(this.logMock.Object, this.servicesConfigMock.Object, this.notificationMock.Object);

            // Act and Assert
            Assert.Equal(tempNotificationEventProcessor.GetAlertListFromString(tempJson).ToArray().Length, numReturnJsonString);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_NotCallExecuteMethod_WhenEmptyEventData()
        {
            // Setup
            this.notificationMock.Setup(a => a.execute()).Returns(Task.CompletedTask);
            var tempEventDataList = new EventData[] { };

            // Act
            this.notificationEventProcessor.ProcessEventsAsync(It.IsAny<PartitionContext>(), tempEventDataList);

            // Assert
            this.notificationMock.Verify(a => a.execute(), Times.Never);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_NotCallExecuteMethod_When_AlertObjectHasEmptyActions()
        {
            // Setup
            this.notificationMock.Setup(a => a.execute()).Returns(Task.CompletedTask);
            var tempEventDataWithOutActions = new EventData(getSamplePayLoadDataWithOutActions());

            // Act
            this.notificationEventProcessor.ProcessEventsAsync(It.IsAny<PartitionContext>(), new EventData[] { tempEventDataWithOutActions });

            // Assert
            this.notificationMock.Verify(e => e.execute(), Times.Never);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(2, 3, 6)]
        [InlineData(1, 1, 1)]
        [InlineData(2, 2, 4)]
        [InlineData(0, 0, 0)]
        public void Should_CallExecuteNxMTimes_When_NEventDataWithMJsonObjectEach(int numEventData, int numJsonString, int numOfCallsToExecuteMethod)
        {
            // Setup
            this.notificationMock.Setup(a => a.execute()).Returns(Task.CompletedTask);
            var tempEventData = new EventData(getSamplePayLoadDataWithNalerts(numJsonString));
            this.notificationMock.Setup(a => a.alarm).Returns(this.getSampleAction());

            // Act
            this.notificationEventProcessor.ProcessEventsAsync(It.IsAny<PartitionContext>(), Enumerable.Repeat<EventData>(tempEventData, numEventData));

            // Assert
            this.notificationMock.Verify(e => e.execute(), Times.Exactly(numOfCallsToExecuteMethod));
        }

        private static byte[] getSamplePayLoadDataWithNalerts(int n)
        {
            var dictionary = new Dictionary<string, object>()
            {
                {"created", "342874237482374" },
                {"modified", "1234123123123" },
                {"rule.description", "Pressure > 380 Aayush" },
                {"rule.severity", "Critical" },
                {"rule.id", "12345" },
                {"rule.actions", new List<object>()
                {
                    new Dictionary<string, object>(){
                        {"Type", "Email" },
                        {"Parameters", new Dictionary<string, object>(){
                            {"Template", "This is a test email."},
                            {"Email", new List<string>(){ "agupta.aayush8484@gmail.com", "t-aagupt@microsoft.com" } }
                        }
                        }
                }
                }
                },
                {"device.id", "213123" },
                {"device.msg.received", "1234123123123" }
            };

            var alertListString = string.Concat(Enumerable.Repeat(JsonConvert.SerializeObject(dictionary), n));
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, alertListString);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream.ToArray();
        }

        private static string getSampleJsonRepeatedNTimes(int n)
        {
            var dictionary = new Dictionary<string, object>()
            {
                {"created", "342874237482374" },
                {"modified", "1234123123123" },
                {"rule.description", "Pressure > 380 Aayush" },
                {"rule.severity", "Critical" },
                {"rule.id", "12345" },
                {"rule.actions", new List<object>()
                {
                    new Dictionary<string, object>()
                    {
                        {"Type", "Email" },
                        {"Parameters", new Dictionary<string, object>(){
                            {"Template", "This is a test email."},
                            {"Email", new List<string>(){ "agupta.aayush8484@gmail.com", "t-aagupt@microsoft.com" } }
                        }
                        }
                }
                }
                },
                {"device.id", "213123" },
                {"device.msg.received", "1234123123123" }

            };
            var jsonString = JsonConvert.SerializeObject(dictionary);
            return String.Join("", Enumerable.Repeat<string>(jsonString, n).ToArray());
        }

        private AlarmNotificationAsaModel getSampleAction()
        {
            return new AlarmNotificationAsaModel()
            {
                Actions = new List<ActionAsaModel>()
                {
                    new ActionAsaModel()
                    {
                        ActionType = "Email",
                        Parameters = new Dictionary<string, object>()
                        {
                            {"Subject", "Test Subject" },
                            {"Email", new Newtonsoft.Json.Linq.JArray() {"email1@outlook.com", "email2@outlook.com"} }
                        }
                    }
                },
                DateCreated = "",
                DateModified = "",
                Device_id = "",
                Message_received = "",
                Rule_description = "",
                Rule_id = "",
                Rule_severity = ""
            };
        }

        private static byte[] getSamplePayLoadDataWithOutActions()
        {
            var dictionary = new Dictionary<string, object>()
            {
                {"created", "342874237482374" },
                {"modified", "1234123123123" },
                {"rule.description", "Pressure > 380 Aayush" },
                {"rule.severity", "Critical" },
                {"rule.id", "12345" },
                {"rule.actions", null },
                {"device.id", "213123" },
                {"device.msg.received", "1234123123123" }
            };
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, JsonConvert.SerializeObject(dictionary));
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream.ToArray();
        }
    }
}
