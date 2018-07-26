// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeviceGroupsAgent.Test.Helpers;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Concurrency;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.EventHub;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Http;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using Xunit;

namespace DeviceGroupsAgent.Test
{
    public class DeviceGroupComponentTests
    {
        private readonly Mock<ILogger> logMock;
        private readonly Mock<IFileWrapper> fileWrapperMock;
        private readonly Mock<ICloudStorageWrapper> cloudStorageWrapperMock;
        private readonly Mock<IDevicesClient> devicesClientMock;

        private readonly IDeviceGroupsWriter deviceGroupsWriter;
        private readonly IDeviceGroupsClient deviceGroupsClient;
        private readonly DateTimeOffset timestamp;

        private readonly string expectedBlobName;
        private readonly string expectedConnectionString;

        private const string REFERENCE_CONTAINER_NAME = "referencedata";
        private const string ACCOUNT_NAME = "blobAccount";
        private const string ACCOUNT_KEY = "xyz";
        private const string ENDPOINT_SUFFIX = "endpoint";
        private const string FILE_NAME = "devicegroups.csv";
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private const string TIME_FORMAT = "HH-mm";
        private const string TEMP_FILE_NAME = "temp.tmp";
        private const string GROUP_ID = "id1";
        private const int TEST_TIMEOUT_MS = 1000;

        public DeviceGroupComponentTests()
        {
            Mock<IBlobStorageConfig> blobStorageConfigMock = new Mock<IBlobStorageConfig>();
            this.SetupBlobStorageConfigMock(blobStorageConfigMock);

            Mock<IServicesConfig> servicesConfigMock = new Mock<IServicesConfig>();
            servicesConfigMock.Setup(x => x.ConfigServiceUrl).Returns("serviceurl");
            servicesConfigMock.Setup(x => x.InitialIotHubManagerRetryIntervalMs).Returns(0);
            servicesConfigMock.Setup(x => x.IotHubManagerRetryCount).Returns(5);
            servicesConfigMock.Setup(x => x.IotHubManagerRetryIntervalIncreaseFactor).Returns(2);

            this.logMock = new Mock<ILogger>();
            this.logMock.Setup(x => x.Warn(It.IsAny<string>(), It.IsAny<Action>()));
            this.logMock.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<Action>()));

            var httpClientMock = new Mock<IHttpClient>();

            this.devicesClientMock = new Mock<IDevicesClient>();

            this.fileWrapperMock = new Mock<IFileWrapper>();
            this.fileWrapperMock.Setup(x => x.GetTempFileName()).Returns(TEMP_FILE_NAME);
            this.fileWrapperMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);

            this.cloudStorageWrapperMock = new Mock<ICloudStorageWrapper>();

            this.deviceGroupsClient = new DeviceGroupsClient(
                httpClientMock.Object,
                this.devicesClientMock.Object,
                servicesConfigMock.Object,
                this.logMock.Object,
                new ThreadWrapper());

            this.timestamp = new DateTime(2018, 4, 20, 10, 0, 0);
            this.expectedBlobName = $"{this.timestamp.ToString(DATE_FORMAT)}/{this.timestamp.ToString(TIME_FORMAT)}/{FILE_NAME}";
            this.expectedConnectionString = $"DefaultEndpointsProtocol=https;AccountName={ACCOUNT_NAME};AccountKey={ACCOUNT_KEY};EndpointSuffix={ENDPOINT_SUFFIX}";

            var blobStorageHelper = new BlobStorageHelper(
                blobStorageConfigMock.Object,
                this.cloudStorageWrapperMock.Object,
                this.logMock.Object);

            this.deviceGroupsWriter = new DeviceGroupsWriter(
                blobStorageConfigMock.Object,
                blobStorageHelper,
                this.fileWrapperMock.Object,
                this.logMock.Object);
        }

        /**
         * Verifies file name is generated as expected as no errors are thrown
         * when device groups writer is called
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void BasicReferenceDataIsWritten()
        {
            // Arrange
            var deviceMapping = new Dictionary<string, IEnumerable<string>>
            {
                ["group1"] = new[] { "device1", "device2", "device3" },
                ["group2"] = new[] { "device6", "device4", "device5" }
            };

            // Act
            this.deviceGroupsWriter.ExportMapToReferenceDataAsync(deviceMapping, this.timestamp);

            // Assert
            this.VerifyFileWrapperMethods(deviceMapping);

            this.VerifyCloudStorageWrapperMethods();

            TestHelperFunctions.VerifyErrorsLogged(this.logMock, 0);
        }

        /**
         * Verifies file name is generated as expected as no errors are thrown
         * when device groups writer is called with an empty dictionary of mappings
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ReferenceDataWrittenWithEmptyMapping()
        {
            // Arrange
            Dictionary<string, IEnumerable<string>> deviceMapping = new Dictionary<string, IEnumerable<string>>();

            // Act
            this.deviceGroupsWriter.ExportMapToReferenceDataAsync(deviceMapping, this.timestamp);

            // Assert
            this.VerifyFileWrapperMethods(deviceMapping);

            this.VerifyCloudStorageWrapperMethods();

            TestHelperFunctions.VerifyErrorsLogged(this.logMock, 0);
        }

        /**
         * Verify if DeviceEventProcessor.ProcessEventsAsync is called, event hub status will
         * be updated to "has seen changes".
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void VerifyDeviceEventProcessorUpdatesEventHubStatus()
        {
            // Arrange
            IEventHubStatus eventHubStatus = new EventHubStatus();
            DeviceEventProcessor deviceEventProcessor = new DeviceEventProcessor(eventHubStatus, 60000, this.logMock.Object);

            EventData data = new EventData(new byte[0]);
            List<EventData> eventDataList = new List<EventData> { data };

            // Act
            deviceEventProcessor.ProcessEventsAsync(null, eventDataList).Wait(TEST_TIMEOUT_MS);

            // Assert
            Assert.True(eventHubStatus.SeenChanges);
            TestHelperFunctions.VerifyErrorsLogged(this.logMock, 0);
        }

        /**
         * Verify DeviceGroupsClient.GetGrouptoDevicesMappingAsync will create expected
         * dictionary and not log errors or warnings if there are no exceptions
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void VerifyGetGroupToDeviceMappingNoExceptions()
        {
            // Arrange
            this.SetupDevicesClientMock();

            // Act
            Dictionary<string, IEnumerable<string>> mapping =
                this.deviceGroupsClient.GetGroupToDevicesMappingAsync(TestHelperFunctions.CreateDeviceGroupListApiModel("etag", GROUP_ID)).Result;

            // Assert
            Assert.True(mapping.ContainsKey(GROUP_ID));
            Assert.Equal(3, mapping[GROUP_ID].Count());

            TestHelperFunctions.VerifyWarningsLogged(this.logMock, 0);
            TestHelperFunctions.VerifyErrorsLogged(this.logMock, 0);
        }

        /**
         * Verify DeviceGroupsClient.GetGrouptoDevicesMappingAsync will not include
         * an empty device group in the device group mapping
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void VerifyGetGroupToDeviceMappingEmptyList()
        {
            // Arrange
            this.SetUpDevicesClientMockEmptyList();

            // Act
            Dictionary<string, IEnumerable<string>> mapping =
                this.deviceGroupsClient.GetGroupToDevicesMappingAsync(TestHelperFunctions.CreateDeviceGroupListApiModel("etag", GROUP_ID)).Result;

            // Assert
            Assert.Empty(mapping.Keys);

            TestHelperFunctions.VerifyWarningsLogged(this.logMock, 0);
            TestHelperFunctions.VerifyErrorsLogged(this.logMock, 0);
        }
        /**
         * Verify DeviceGroupsClient.GetGrouptoDevicesMappingAsync will retry
         * devices query on exception, and succeed if there are fewer than 5 exceptions
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void VerifyGetGroupToDeviceMappingRetriesOnException()
        {
            // Arrange
            this.Setup4ExceptionSequenceDevicesClientMock();

            // Act
            Dictionary<string, IEnumerable<string>> mapping =
                this.deviceGroupsClient.GetGroupToDevicesMappingAsync(TestHelperFunctions.CreateDeviceGroupListApiModel("etag", GROUP_ID)).Result;

            // Assert
            Assert.True(mapping.ContainsKey(GROUP_ID));
            Assert.Equal(3, mapping[GROUP_ID].Count());
            TestHelperFunctions.VerifyWarningsLogged(this.logMock, 4);
            TestHelperFunctions.VerifyErrorsLogged(this.logMock, 0);
        }

        /**
         * Verify DeviceGroupsClient.GetGrouptoDevicesMappingAsync will fail if it
         * gets exceptions on getDevices 5 times
         */
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void VerifyGetGroupToDeviceMappingFailsAfter5Retries()
        {
            // Arrange
            this.SetupAllExceptionSequenceDevicesClientMock();

            // Act
            Exception ex = Assert.Throws<AggregateException>(() =>
                this.deviceGroupsClient.GetGroupToDevicesMappingAsync(TestHelperFunctions.CreateDeviceGroupListApiModel("etag", GROUP_ID)).Wait());

            // Assert
            Assert.IsType<ExternalDependencyException>(ex.InnerException);
            TestHelperFunctions.VerifyWarningsLogged(this.logMock, 5);
            TestHelperFunctions.VerifyErrorsLogged(this.logMock, 1);
        }

        // Verify all cloud storage methods are called correctly
        private void VerifyCloudStorageWrapperMethods()
        {
            this.cloudStorageWrapperMock.Verify(c => c.Parse(this.expectedConnectionString));
            this.cloudStorageWrapperMock.Verify(c => c.CreateCloudBlobClient(It.IsAny<CloudStorageAccount>()));
            this.cloudStorageWrapperMock.Verify(c => c.GetContainerReference(It.IsAny<CloudBlobClient>(), REFERENCE_CONTAINER_NAME));
            this.cloudStorageWrapperMock.Verify(c => c.CreateIfNotExistsAsync(
                It.IsAny<CloudBlobContainer>(),
                BlobContainerPublicAccessType.Blob,
                It.IsAny<BlobRequestOptions>(),
                It.IsAny<OperationContext>()));
            this.cloudStorageWrapperMock.Verify(c => c.GetBlockBlobReference(It.IsAny<CloudBlobContainer>(), this.expectedBlobName));
        }

        // Verify file is written correctly in csv format
        private void VerifyFileWrapperMethods(Dictionary<string, IEnumerable<string>> deviceMapping)
        {
            this.fileWrapperMock.Verify(f => f.WriteLine(It.IsAny<StreamWriter>(), "\"DeviceId\",\"GroupId\""));
            foreach (string key in deviceMapping.Keys)
            {
                foreach (string value in deviceMapping[key])
                {
                    this.fileWrapperMock.Verify(f => f.WriteLine(It.IsAny<StreamWriter>(), $"\"{value}\",\"{key}\""), Times.Once());
                }
            }
            this.fileWrapperMock.Verify(f => f.Delete(TEMP_FILE_NAME));
        }

        private void SetupBlobStorageConfigMock(Mock<IBlobStorageConfig> blobStorageConfigMock)
        {
            blobStorageConfigMock.Setup(x => x.ReferenceDataDateFormat).Returns(DATE_FORMAT);
            blobStorageConfigMock.Setup(x => x.ReferenceDataTimeFormat).Returns(TIME_FORMAT);
            blobStorageConfigMock.Setup(x => x.ReferenceDataDeviceGroupsFileName).Returns(FILE_NAME);
            blobStorageConfigMock.Setup(x => x.AccountName).Returns(ACCOUNT_NAME);
            blobStorageConfigMock.Setup(x => x.AccountKey).Returns(ACCOUNT_KEY);
            blobStorageConfigMock.Setup(x => x.EndpointSuffix).Returns(ENDPOINT_SUFFIX);
            blobStorageConfigMock.Setup(x => x.ReferenceDataContainer).Returns(REFERENCE_CONTAINER_NAME);
        }

        // Set up device client mock to return dummy device list with no exceptions
        private void SetupDevicesClientMock()
        {
            this.devicesClientMock
                .Setup(x => x.GetListAsync(It.IsAny<IEnumerable<DeviceGroupConditionApiModel>>()))
                .Returns(Task.FromResult(this.GetDeviceList()));
        }

        private void SetUpDevicesClientMockEmptyList()
        {
            IEnumerable<string> result = new List<string>();
            this.devicesClientMock
                .Setup(x => x.GetListAsync(It.IsAny<IEnumerable<DeviceGroupConditionApiModel>>()))
                .Returns(Task.FromResult(result));
        }

        /**
         * Set up sequence for devices client that will throw 4 exceptions,
         * then return the device list
         */
        private void Setup4ExceptionSequenceDevicesClientMock()
        {
            this.devicesClientMock
                .SetupSequence(x => x.GetListAsync(It.IsAny<IEnumerable<DeviceGroupConditionApiModel>>()))
                .Throws<Exception>()
                .Throws<Exception>()
                .Throws<Exception>()
                .Throws<Exception>()
                .Returns(Task.FromResult(this.GetDeviceList()));
        }

        // Set up sequence for devices client that will throw 5 exceptions
        private void SetupAllExceptionSequenceDevicesClientMock()
        {
            this.devicesClientMock
                .SetupSequence(x => x.GetListAsync(It.IsAny<IEnumerable<DeviceGroupConditionApiModel>>()))
                .Throws<Exception>()
                .Throws<Exception>()
                .Throws<Exception>()
                .Throws<Exception>()
                .Throws<Exception>();
        }

        // Returns dummy list of device ids
        private IEnumerable<string> GetDeviceList()
        {
            return new List<string>
            {
                "device1",
                "device2",
                "device3"
            };
        }
    }
}
