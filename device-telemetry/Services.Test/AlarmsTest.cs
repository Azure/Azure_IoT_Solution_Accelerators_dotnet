// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Moq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class AlarmsTest
    {
        private readonly Mock<IStorageClient> storageClient;
        private readonly Mock<ILogger> logger;
        private readonly IAlarms alarms;

        public AlarmsTest()
        {
            var servicesConfig = new ServicesConfig
            {
                AlarmsConfig = new AlarmsConfig("database", "collection", 3)
            };
            this.storageClient = new Mock<IStorageClient>();
            this.logger = new Mock<ILogger>();
            this.alarms = new Alarms(servicesConfig, this.storageClient.Object, this.logger.Object);
        }

        /**
         * Test basic functionality of delete alarms by id.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void BasicDelete()
        {
            // Arrange
            List<string> ids = new List<string> { "id1", "id2", "id3", "id4" };
            Document d1 = new Document
            {
                Id = "test"
            };
            this.storageClient
                .Setup(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult(d1));

            // Act
            this.alarms.Delete(ids);

            // Assert
            for (int i = 0; i < ids.Count; i++)
            {
                this.storageClient.Verify(x => x.DeleteDocumentAsync("database", "collection", ids[i]), Times.Once);
            }

            this.logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Never);
            this.logger.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Never);
        }

        /**
         * Verify if delete alarm by id fails once it will retry
        */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task DeleteSucceedsTransientExceptionAsync()
        {
            // Arrange
            List<string> ids = new List<string> { "id1" };
            Document d1 = new Document
            {
                Id = "test"
            };
            this.storageClient
                .SetupSequence(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new Exception())
                .Returns(Task.FromResult(d1));

            // Act
            await this.alarms.Delete(ids);

            // Assert
            this.storageClient.Verify(x => x.DeleteDocumentAsync("database", "collection", ids[0]), Times.Exactly(2));

            this.logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Never);
            this.logger.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Once);
        }

        /**
         * Verify that after 3 failures to delete an alarm an
         * exception will be thrown.
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task DeleteFailsAfter3ExceptionsAsync()
        {
            // Arrange
            List<string> ids = new List<string> { "id1" };
            Document d1 = new Document
            {
                Id = "test"
            };

            this.storageClient
                .SetupSequence(x => x.DeleteDocumentAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new Exception())
                .Throws(new Exception())
                .Throws(new Exception());

            // Act
            await Assert.ThrowsAsync<ExternalDependencyException>(async () => await this.alarms.Delete(ids));

            // Assert
            this.storageClient.Verify(x => x.DeleteDocumentAsync("database", "collection", ids[0]), Times.Exactly(3));

            this.logger.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Once);
            this.logger.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Exactly(2));
        }
    }
}
