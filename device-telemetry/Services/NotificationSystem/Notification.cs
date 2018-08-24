// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public interface INotification
    {
        Task execute();
        AlarmNotificationAsaModel alarm { get; set; }
    }

    public class Notification : INotification
    {
        private const string EMAIL_STRING = "Email";
        private const string TEMPLATE_STRING = "Template";

        private const EmailImplementationTypes EMAIL_IMPLEMENTATION_TYPE = EmailImplementationTypes.LogicApp;
        private readonly IImplementationWrapper implementationWrapper;
        private ILogger logger;
        private INotificationImplementation implementation;

        public AlarmNotificationAsaModel alarm { get; set; }

        public Notification(IImplementationWrapper implementationWrapper,
                            ILogger logger)
        {
            this.implementationWrapper = implementationWrapper;
            this.logger = logger;
        }

        public async Task execute()
        {
            foreach (ActionAsaModel action in this.alarm.Actions)
            {
                switch (action.ActionType)
                {
                    case EMAIL_STRING:
                        implementation = this.implementationWrapper.GetImplementationType(EMAIL_IMPLEMENTATION_TYPE);
                        break;
                }
                implementation.SetMessage((string)action.Parameters[TEMPLATE_STRING], this.alarm.Rule_id, this.alarm.Rule_description);
                if (action.Parameters[EMAIL_STRING] != null) implementation.SetReceiver(((JArray)action.Parameters[EMAIL_STRING]).ToObject<List<string>>());
                if (await implementation.Execute() == 0)
                {
                    this.logger.Error("Error executing the action", () => { });
                }
            }
        }
    }
}

public enum EmailImplementationTypes
{
    LogicApp
}

