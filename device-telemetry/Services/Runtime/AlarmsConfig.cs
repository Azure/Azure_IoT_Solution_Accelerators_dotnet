// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public class AlarmsConfig
    {
        public StorageConfig StorageConfig { get; set; }
        public int MaxDeleteRetries { get; set; }

        public AlarmsConfig(
            string documentDbDatabase,
            string documentDbCollection,
            int maxDeleteRetries)
        {
            this.StorageConfig = new StorageConfig(documentDbDatabase, documentDbCollection);
            this.MaxDeleteRetries = maxDeleteRetries;
        }
    }
}
