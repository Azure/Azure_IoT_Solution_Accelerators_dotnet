// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.EventHub
{
    public class DeviceEventProcessorFactory : IEventProcessorFactory
    {
        private readonly IEventHubStatus eventHubStatus;
        private readonly int checkpointTimeInMs;
        private readonly ILogger logger;

        public DeviceEventProcessorFactory(
            IEventHubStatus eventHubStatus,
            IServicesConfig servicesConfig,
            ILogger logger)
        {
            this.eventHubStatus = eventHubStatus;
            this.checkpointTimeInMs = servicesConfig.EventHubCheckpointTimeMs;
            this.logger = logger;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new DeviceEventProcessor(this.eventHubStatus, this.checkpointTimeInMs, this.logger);
        }
    }
}