// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Http;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class UserManagementClientTest
    {
        private const string MOCK_SERVICE_URI = @"http://mockauth";

        private readonly Mock<IHttpClient> mockHttpClient;
        private readonly UserManagementClient client;
        private readonly Random rand;

        public UserManagementClientTest()
        {
            this.mockHttpClient = new Mock<IHttpClient>();
            this.client = new UserManagementClient(
                this.mockHttpClient.Object,
                new ServicesConfig
                {
                    UserManagementApiUrl = MOCK_SERVICE_URI
                },
                new Logger("UnitTest", LogLevel.Debug));
            this.rand = new Random();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllowedActions_ReturnValues()
        {
            var userObjectId = this.rand.NextString();
            var roles = new List<string> { "Admin" };
            var allowedActions = new List<string> { "CreateDevices", "UpdateDevices" };

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccessStatusCode = true,
                Content = JsonConvert.SerializeObject(allowedActions)
            };

            this.mockHttpClient
                .Setup(x => x.PostAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            var result = await this.client.GetAllowedActionsAsync(userObjectId, roles);

            this.mockHttpClient
                .Verify(x => x.PostAsync(It.Is<IHttpRequest>(r => r.Check($"{MOCK_SERVICE_URI}/users/{userObjectId}/allowedActions"))), Times.Once);

            Assert.Equal(allowedActions, result);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllowedActions_ReturnNotFound()
        {
            var userObjectId = this.rand.NextString();
            var roles = new List<string> { "Unknown" };

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccessStatusCode = false
            };

            this.mockHttpClient
                .Setup(x => x.PostAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            await Assert.ThrowsAsync<ResourceNotFoundException>(async () =>
                await this.client.GetAllowedActionsAsync(userObjectId, roles));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetAllowedActions_ReturnError()
        {
            var userObjectId = this.rand.NextString();
            var roles = new List<string> { "Unknown" };

            var response = new HttpResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccessStatusCode = false
            };

            this.mockHttpClient
                .Setup(x => x.PostAsync(It.IsAny<IHttpRequest>()))
                .ReturnsAsync(response);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await this.client.GetAllowedActionsAsync(userObjectId, roles));
        }
    }
}
