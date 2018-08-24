// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation
{
    public interface INotificationImplementation
    {
        void SetReceiver(List<string> receivers);
        void SetMessage(string message, string ruleId, string ruleDescription);
        Task<HttpStatusCode> Execute();
    }
}
