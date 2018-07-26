// Copyright (c) Microsoft. All rights reserved.

using System;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public class StorageConfig
    {
        public string DocumentDbDatabase { get; set; }
        public string DocumentDbCollection { get; set; }

        public StorageConfig(
            string documentDbDatabase,
            string documentDbCollection)
        {
            this.DocumentDbDatabase = documentDbDatabase;
            if (string.IsNullOrEmpty(this.DocumentDbDatabase))
            {
                throw new Exception("DocumentDb database name is empty in configuration");
            }

            this.DocumentDbCollection = documentDbCollection;
            if (string.IsNullOrEmpty(this.DocumentDbCollection))
            {
                throw new Exception("DocumentDb collection name is empty in configuration");
            }
        }
    }
}
