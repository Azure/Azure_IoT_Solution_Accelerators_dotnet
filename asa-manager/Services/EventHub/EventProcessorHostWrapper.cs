// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.EventHubs.Processor;

// Wrapper for unit testing purposes
namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.EventHub
{
    public interface IEventProcessorHostWrapper
    {
        EventProcessorHost CreateEventProcessorHost(
            string eventHubPath,
            string consumerGroupName,
            string eventHubConnectionString,
            string storageConnectionString,
            string leaseContainerName);

        Task RegisterEventProcessorFactoryAsync(EventProcessorHost host, IEventProcessorFactory factory);
    }

    public class EventProcessorHostWrapper : IEventProcessorHostWrapper
    {
        public EventProcessorHost CreateEventProcessorHost(
            string eventHubPath,
            string consumerGroupName,
            string eventHubConnectionString,
            string storageConnectionString,
            string leaseContainerName)
        {
            return new EventProcessorHost(eventHubPath, consumerGroupName, eventHubConnectionString, storageConnectionString, leaseContainerName);
        }

        public Task RegisterEventProcessorFactoryAsync(EventProcessorHost host, IEventProcessorFactory factory)
        {
            return host.RegisterEventProcessorFactoryAsync(factory);
        }
    }
}
