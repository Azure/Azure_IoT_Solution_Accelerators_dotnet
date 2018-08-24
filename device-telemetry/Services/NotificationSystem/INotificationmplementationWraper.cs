// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public interface IImplementationWrapper
    {
        INotificationImplementation GetImplementationType(EmailImplementationTypes actionType);
    }

    public class ImplementationWraper : IImplementationWrapper
    {
        private readonly IServicesConfig servicesConfig;
        private readonly IHttpClient httpClient;
        private readonly IHttpRequest httpRequest;
        private readonly ILogger logger;

        public ImplementationWraper(IServicesConfig servicesConfig, IHttpRequest httpRequest, IHttpClient httpClient, ILogger logger)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
            this.httpClient = httpClient;
            this.httpRequest = httpRequest;
        }

        public INotificationImplementation GetImplementationType(EmailImplementationTypes actionType)
        {
            switch (actionType)
            {
                case EmailImplementationTypes.LogicApp:
                    return new LogicApp(this.servicesConfig.LogicAppEndPointUrl, this.servicesConfig.SolutionName, this.httpRequest, this.httpClient, this.logger);
            }
            return null;
        }
    }
}
