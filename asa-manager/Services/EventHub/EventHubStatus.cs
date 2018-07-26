// Copyright (c) Microsoft. All rights reserved.

// Stores whether event hub has seen changes
namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.EventHub
{
    public interface IEventHubStatus
    {
        bool SeenChanges { get; set; }
    }

    public class EventHubStatus : IEventHubStatus
    {
        public bool SeenChanges { get; set; }
    }
}
