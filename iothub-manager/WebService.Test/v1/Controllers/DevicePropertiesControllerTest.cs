// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using Moq;
using WebService.Test.helpers;
using Xunit;

namespace WebService.Test.v1.Controllers
{

    public class DevicePropertiesControllerTest
    {
        private readonly DevicePropertiesController devicePropertiesController;
        private readonly Mock<IDeviceProperties> devicePropertiesMock;

        public DevicePropertiesControllerTest()
        {
            this.devicePropertiesMock = new Mock<IDeviceProperties>();
            this.devicePropertiesController = new DevicePropertiesController(this.devicePropertiesMock.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetPropertiesReturnExpectedResponse()
        {
            // Arrange
            this.devicePropertiesMock.Setup(x => x.GetListAsync()).ReturnsAsync(this.CreateFakeList());
            DevicePropertiesApiModel expectedModel = new DevicePropertiesApiModel(this.CreateFakeList());

            // Act
            DevicePropertiesApiModel model = await this.devicePropertiesController.GetAsync();

            // Assert
            this.devicePropertiesMock.Verify(x => x.GetListAsync(), Times.Once);
            Assert.NotNull(model);
            Assert.Equal(model.Metadata.Keys, expectedModel.Metadata.Keys);
            foreach (string key in model.Metadata.Keys)
            {
                Assert.Equal(model.Metadata[key], expectedModel.Metadata[key]);
            }
            // Assert model and expected model have same items
            Assert.Empty(model.Items.Except(expectedModel.Items));
            Assert.Empty(expectedModel.Items.Except(model.Items));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetPropertiesThrowsException_IfDevicePropertiesThrowsException()
        {
            // Arrange
            this.devicePropertiesMock.Setup(x => x.GetListAsync()).Throws<ExternalDependencyException>();

            // Act - Assert
            await Assert.ThrowsAsync<ExternalDependencyException>(() => this.devicePropertiesController.GetAsync());

        }

        private List<String> CreateFakeList()
        {
            return new List<string>
            {
                "property1",
                "property2",
                "property3",
                "property4"
            };
        }
    }
}
