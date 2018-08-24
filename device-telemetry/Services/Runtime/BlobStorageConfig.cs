// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public interface IBlobStorageConfig
    {
        // Azure blob credentials
        string AccountName { get; }
        string AccountKey { get; }
        string EndpointSuffix { get; }

        // TODO: add docs
        string EventHubContainer { get; set; }
    }

    public class BlobStorageConfig : IBlobStorageConfig
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string EndpointSuffix { get; set; }

        public string EventHubContainer { get; set; }
    }
}
