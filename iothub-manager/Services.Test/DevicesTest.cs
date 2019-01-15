// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Moq;
using Services.Test.helpers;
using Xunit;
using AuthenticationType = Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models.AuthenticationType;

namespace Services.Test
{
    public class DevicesTest
    {
        private readonly IDevices devices;
        private readonly Mock<RegistryManager> registryMock;
        private readonly string ioTHubHostName = "ioTHubHostName";

        public DevicesTest()
        {
            this.registryMock = new Mock<RegistryManager>();
            this.devices = new Devices(registryMock.Object, this.ioTHubHostName);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("", "", true)]
        [InlineData("asdf", "", true)]
        [InlineData("", "qwer", true)]
        [InlineData("asdf", "qwer", false)]
        public async Task GetModuleTwinTest(string deviceId, string moduleId, bool throwsException)
        {
            if (throwsException)
            {
                // Act & Assert
                await Assert.ThrowsAsync<InvalidInputException>(async () =>
                    await this.devices.GetModuleTwinAsync(deviceId, moduleId));
            }
            else
            {
                // Arrange
                this.registryMock
                    .Setup(x => x.GetTwinAsync(deviceId, moduleId))
                    .ReturnsAsync(DevicesTest.CreateTestTwin(0));

                // Act
                var twinSvcModel = await this.devices.GetModuleTwinAsync(deviceId, moduleId);

                // Assert
                Assert.Equal("value0", twinSvcModel.ReportedProperties["test"].ToString());
                Assert.Equal("value0", twinSvcModel.DesiredProperties["test"].ToString());
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("", 5)]
        [InlineData("2", 5)]
        [InlineData("6", 5)]
        public async Task TwinByQueryContinuationTest(string continuationToken, int numResults)
        {
            // Arrange
            this.registryMock
                .Setup(x => x.CreateQuery(It.IsAny<string>()))
                .Returns(new ResultQuery(numResults));

            // Act
            var queryResult = await this.devices.GetModuleTwinsByQueryAsync("", continuationToken);

            // Assert
            Assert.Equal("continuationToken", queryResult.ContinuationToken);

            var startIndex = string.IsNullOrEmpty(continuationToken) ? 0 : int.Parse(continuationToken);
            var total = Math.Max(0, numResults - startIndex);
            Assert.Equal(total, queryResult.Items.Count);

            for (int i = 0; i < total; i++)
            {
                var expectedValue = "value" + (i + startIndex);
                Assert.Equal(expectedValue, queryResult.Items[i].ReportedProperties["test"].ToString());
                Assert.Equal(expectedValue, queryResult.Items[i].DesiredProperties["test"].ToString());
            }
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("", "SELECT * FROM devices.modules")]
        [InlineData("deviceId='test'", "SELECT * FROM devices.modules where deviceId='test'")]
        public async Task GetTwinByQueryTest(string query, string queryToMatch)
        {
            // Arrange
            this.registryMock
                .Setup(x => x.CreateQuery(queryToMatch))
                .Returns(new ResultQuery(3));

            // Act
            var queryResult = await this.devices.GetModuleTwinsByQueryAsync(query, "");

            // Assert
            Assert.Equal("continuationToken", queryResult.ContinuationToken);
            Assert.Equal(3, queryResult.Items.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetEdgeDeviceTest()
        {
            // Arrange
            var nonEdgeDevice = "nonEdgeDevice";
            var edgeDevice = "edgeDevice";
            var edgeDeviceFromTwin = "edgeDeviceFromTwin";

            this.registryMock
                .Setup(x => x.CreateQuery(It.IsAny<string>()))
                .Returns(new ResultQuery(0));

            this.registryMock
                .Setup(x => x.GetTwinAsync(nonEdgeDevice))
                .ReturnsAsync(DevicesTest.CreateTestTwin(0));
            this.registryMock
                .Setup(x => x.GetTwinAsync(edgeDevice))
                .ReturnsAsync(DevicesTest.CreateTestTwin(1));
            this.registryMock
                .Setup(x => x.GetTwinAsync(edgeDeviceFromTwin))
                .ReturnsAsync(DevicesTest.CreateTestTwin(2, true));

            this.registryMock
                .Setup(x => x.GetDeviceAsync(nonEdgeDevice))
                .ReturnsAsync(DevicesTest.CreateTestDevice("nonEdgeDevice", false));
            this.registryMock
                .Setup(x => x.GetDeviceAsync(edgeDevice))
                .ReturnsAsync(DevicesTest.CreateTestDevice("edgeDevice", true));
            this.registryMock
                .Setup(x => x.GetDeviceAsync(edgeDeviceFromTwin))
                .ReturnsAsync(DevicesTest.CreateTestDevice("edgeDeviceFromTwin", false));

            // Act
            var dvc1 = await this.devices.GetAsync(nonEdgeDevice);
            var dvc2 = await this.devices.GetAsync(edgeDevice);
            var dvc3 = await this.devices.GetAsync(edgeDeviceFromTwin);

            // Assert
            Assert.False(dvc1.IsEdgeDevice, "Non-edge device reporting edge device");
            Assert.True(dvc2.IsEdgeDevice, "Edge device reported not edge device");

            // When using getDevices method which is deprecated it doesn't return IsEdgeDevice
            // capabilities properly so we support grabbing this from the device twin as well.
            Assert.True(dvc3.IsEdgeDevice, "Edge device from twin reporting not edge device");
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task TestConnectedEdgeDevice()
        {
            // Arrange
            // Set only 3 of the devices to be marked as connected
            // The first two are non-edge devices so it shouldn't be listed
            // as connected in the result
            var twins = new List<Twin>()
            {
                CreateTestTwin(0, false),
                CreateTestTwin(1, false),
                CreateTestTwin(2, true),
                CreateTestTwin(3, true),
            };

            this.registryMock
                .Setup(x => x.CreateQuery(It.Is<string>(s => s.Equals("SELECT * FROM devices"))))
                .Returns(new ResultQuery(twins));

            this.registryMock
                .Setup(x => x.CreateQuery(It.Is<string>(s => s.Equals("SELECT * FROM devices where connectionState = 'Connected'"))))
                .Returns(new ResultQuery(twins));

            // Act
            var allDevices = await this.devices.GetListAsync("", "");

            // Assert
            Assert.Equal(4, allDevices.Items.Count);
            Assert.False(allDevices.Items[0].Connected || allDevices.Items[1].Connected);
            Assert.True(allDevices.Items[2].Connected);
            Assert.False(allDevices.Items[3].Connected);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData("SelfSigned")]
        [InlineData("CertificateAuthority")]
        public async Task InvalidAuthenticationTypeForEdgeDevice(string authTypeString)
        {
            // Arrange
            var authType = Enum.Parse<AuthenticationType>(authTypeString);

            var auth = new AuthenticationMechanismServiceModel()
            {
                AuthenticationType = authType
            };

            DeviceServiceModel model = new DeviceServiceModel
            (
                etag: "etag",
                id: "deviceId",
                c2DMessageCount: 0,
                lastActivity: DateTime.Now,
                connected: true,
                enabled: true,
                isEdgeDevice: true,
                lastStatusUpdated: DateTime.Now,
                twin: null,
                ioTHubHostName: this.ioTHubHostName,
                authentication: auth
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidInputException>(async () =>
                await this.devices.CreateAsync(model));
        }

        private static Twin CreateTestTwin(int valueToReport, bool isEdgeDevice = false)
        {
            var twin = new Twin()
            {
                DeviceId = valueToReport.ToString(),
                Properties = new TwinProperties(),
                Capabilities = isEdgeDevice ? new DeviceCapabilities() { IotEdge = true } : null
            };
            twin.Properties.Reported = new TwinCollection("{\"test\":\"value" + valueToReport + "\"}");
            twin.Properties.Desired = new TwinCollection("{\"test\":\"value" + valueToReport + "\"}");
            return twin;
        }

        private static Device CreateTestDevice(string deviceId, bool isEdgeDevice)
        {
            var dvc = new Device(deviceId)
            {
                Authentication = new AuthenticationMechanism
                {
                    Type = Microsoft.Azure.Devices.AuthenticationType.Sas,
                    SymmetricKey = new SymmetricKey
                    {
                        PrimaryKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("SomeTestPrimaryKey")),
                        SecondaryKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("SomeTestSecondaryKey"))
                    }
                },
                Capabilities = isEdgeDevice ? new DeviceCapabilities() {IotEdge = true} : null
            };
            return dvc;
        }

        /// <summary>
        /// Returns a set of edge and non-edge devices
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Device> CreateTestListOfDevices()
        {
            return new List<Device>()
            {
                DevicesTest.CreateTestDevice("device0", false),
                DevicesTest.CreateTestDevice("device1", false),
                DevicesTest.CreateTestDevice("device2", true),
                DevicesTest.CreateTestDevice("device3", true),
                DevicesTest.CreateTestDevice("device4", false),
                DevicesTest.CreateTestDevice("device5", true),
            };
        }
    }
}
