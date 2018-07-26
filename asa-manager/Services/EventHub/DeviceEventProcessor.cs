// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.EventHub
{
    public class DeviceEventProcessor : IEventProcessor
    {
        private readonly IEventHubStatus eventHubStatus;
        private readonly ILogger logger;
        private readonly Stopwatch checkpointStopwatch;
        private readonly int checkpointTimeInMs;

        public DeviceEventProcessor(
            IEventHubStatus eventHubStatus,
            int checkpointTimeInMs,
            ILogger logger)
        {
            this.eventHubStatus = eventHubStatus;
            this.logger = logger;
            this.checkpointStopwatch = new Stopwatch();
            this.checkpointTimeInMs = checkpointTimeInMs;
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            this.logger.Debug("Event Processor Shutting Down.", () => new { context.PartitionId, reason });
            await context.CheckpointAsync();
        }

        public Task OpenAsync(PartitionContext context)
        {
            this.logger.Debug("Event Processor initialized.", () => new { context.PartitionId });
            this.checkpointStopwatch.Start();
            return Task.CompletedTask;
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            if (messages.Any())
            {
                this.eventHubStatus.SeenChanges = true;
            }

            if (this.checkpointStopwatch.ElapsedMilliseconds >= this.checkpointTimeInMs)
            {
                await context.CheckpointAsync();
                this.checkpointStopwatch.Restart();
            }
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            this.logger.Error("Event processor received error", () => new { context.PartitionId, error.Message });
            return Task.CompletedTask;
        }
    }
}
