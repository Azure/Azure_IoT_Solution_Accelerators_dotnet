// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public class NotificationEventProcessorFactory : IEventProcessorFactory
    {
        private readonly ILogger logger;
        private readonly IServicesConfig servicesConfig;
        private readonly INotification notification;

        public NotificationEventProcessorFactory(
            ILogger logger,
            IServicesConfig servicesConfig,
            INotification notification)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
            this.notification = notification;
        }
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new NotificationEventProcessor(this.logger, this.servicesConfig, this.notification);
        }
    }
}
