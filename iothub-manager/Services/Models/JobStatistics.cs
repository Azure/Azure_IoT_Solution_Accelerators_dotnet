// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Devices;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    /// <summary>
    /// refer to Azure.Devices.DeviceJobStatistics
    /// </summary>
    public class JobStatistics
    {
        public JobStatistics()
        {
        }

        public JobStatistics(DeviceJobStatistics azureModel)
        {
            this.DeviceCount = azureModel.DeviceCount;
            this.FailedCount = azureModel.FailedCount;
            this.SucceededCount = azureModel.SucceededCount;
            this.RunningCount = azureModel.RunningCount;
            this.PendingCount = azureModel.PendingCount;
        }

        [JsonProperty(PropertyName = "DeviceCount")]
        public int DeviceCount { get; set; }

        [JsonProperty(PropertyName = "FailedCount")]
        public int FailedCount { get; set; }

        [JsonProperty(PropertyName = "SucceededCount")]
        public int SucceededCount { get; set; }

        [JsonProperty(PropertyName = "RunningCount")]
        public int RunningCount { get; set; }

        [JsonProperty(PropertyName = "PendingCount")]
        public int PendingCount { get; set; }
    }
}
