// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DevicePropertyServiceModel
    {
        public bool Rebuilding { get; set; } = false;

        public HashSet<string> Tags { get; set; }

        public HashSet<string> Reported { get; set; }

        public bool IsNullOrEmpty() => (Tags == null || Tags.Count == 0) && (Reported == null || Reported.Count == 0);
    }
}
