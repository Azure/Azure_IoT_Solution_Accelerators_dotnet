// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.NotificationSystem
{
    public class AgentTest
    {
        private readonly Mock<ILogger> logMock;
        private readonly Mock<IServicesConfig> servicesConfigMock;
        private readonly Mock<IEventProcessorHostWrapper> eventProcessorHostWrapperMock;
        private readonly Mock<IBlobStorageConfig> blobStorageConfigMock;
        private readonly Mock<IEventProcessorFactory> eventProcessorFactoryMock;
        private readonly IAgent notificationSystemAgent;

        private CancellationTokenSource agentsRunState;
        private CancellationToken runState;

        public AgentTest()
        {
            this.logMock = new Mock<ILogger>();
            this.servicesConfigMock = new Mock<IServicesConfig>();
            this.blobStorageConfigMock = new Mock<IBlobStorageConfig>();
            this.eventProcessorFactoryMock = new Mock<IEventProcessorFactory>();
            this.eventProcessorHostWrapperMock = new Mock<IEventProcessorHostWrapper>();

            this.notificationSystemAgent = new Agent(this.logMock.Object, this.servicesConfigMock.Object, this.blobStorageConfigMock.Object,
                                                    this.eventProcessorHostWrapperMock.Object, this.eventProcessorFactoryMock.Object);
            this.agentsRunState = new CancellationTokenSource();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_RegisterEventProcessorFactory_When_CancellationTokenIsFalse()
        {
            // Arrange
            this.runState = this.agentsRunState.Token;
            this.eventProcessorHostWrapperMock.Setup(x => x.RegisterEventProcessorFactoryAsync(It.IsAny<EventProcessorHost>(),
                It.IsAny<IEventProcessorFactory>())).Returns(Task.CompletedTask);

            // Act
            this.notificationSystemAgent.RunAsync(this.runState);

            // Assert
            this.eventProcessorHostWrapperMock.Verify(a => a.RegisterEventProcessorFactoryAsync(It.IsAny<EventProcessorHost>(),
                It.IsAny<IEventProcessorFactory>()), Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_ThrowException_When_InvalidCredentialsPassedToRegisterEventProcessorFactory()
        {
            // Arrange
            this.runState = this.agentsRunState.Token;
            this.eventProcessorHostWrapperMock.Setup(x => x.RegisterEventProcessorFactoryAsync(It.IsAny<EventProcessorHost>(), It.IsAny<IEventProcessorFactory>())).Returns(Task.FromException(new Exception()));

            // Act
            this.notificationSystemAgent.RunAsync(this.runState);

            // Assert
            Assert.ThrowsAsync<Exception>(() => this.eventProcessorHostWrapperMock.Object.RegisterEventProcessorFactoryAsync(It.IsAny<EventProcessorHost>(),
                It.IsAny<IEventProcessorFactory>()));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_NotRegisterEventProcessorFactory_When_CancellationTokenIsTrue()
        {
            // Arrange
            this.agentsRunState.Cancel();
            this.runState = this.agentsRunState.Token;

            // Act
            this.notificationSystemAgent.RunAsync(this.runState);

            // Assert
            this.eventProcessorHostWrapperMock.Verify(a => a.CreateEventProcessorHost(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            this.eventProcessorHostWrapperMock.Verify(a => a.RegisterEventProcessorFactoryAsync(It.IsAny<EventProcessorHost>(),
                It.IsAny<IEventProcessorFactory>()), Times.Never);
        }
    }
}
