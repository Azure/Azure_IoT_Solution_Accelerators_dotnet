// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Moq;

namespace DeviceGroupsAgent.Test.Helpers
{
    public static class TestHelperFunctions
    {
        // Verify no errors are thrown by log
        public static void VerifyErrorsLogged(Mock<ILogger> logMock, int numErrors)
        {
            logMock.Verify(l => l.Error(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Exactly(numErrors));
        }

        // Verify no errors are thrown by log
        public static void VerifyWarningsLogged(Mock<ILogger> logMock, int numWarnings)
        {
            logMock.Verify(l => l.Warn(It.IsAny<string>(), It.IsAny<Func<object>>()), Times.Exactly(numWarnings));
        }

        // Create fake device group list with one group definition with no conditions
        public static DeviceGroupListApiModel CreateDeviceGroupListApiModel(string etag, string groupId)
        {
            var returnObj = new DeviceGroupListApiModel();
            var itemsList = new List<DeviceGroupApiModel>
            {
                new DeviceGroupApiModel()
                {
                    Id = groupId,
                    Conditions = new DeviceGroupConditionApiModel[0],
                    DisplayName = "display",
                    ETag = etag
                }
            };
            returnObj.Items = itemsList;
            return returnObj;
        }
    }
}
