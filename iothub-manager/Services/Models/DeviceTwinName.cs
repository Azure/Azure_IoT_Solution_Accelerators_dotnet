// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeviceTwinName
    {
        public HashSet<string> Tags { get; set; }

        public HashSet<string> ReportedProperties { get; set; }
    }
}
