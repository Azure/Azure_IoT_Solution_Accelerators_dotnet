// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime
{
    public interface IServicesConfig
    {
        string IoTHubConnString { get; set; }
        string StorageAdapterApiUrl { get; }
        string DevicePropertiesWhiteList { get; }
        // ReSharper disable once InconsistentNaming
        long DevicePropertiesTTL { get; }
        long DevicePropertiesRebuildTimeout { get; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string IoTHubConnString { get; set; }
        public string StorageAdapterApiUrl { get; set; }
        public string ConfigServiceUri { get; set; }
        public string DevicePropertiesWhiteList { get; set; }
        public long DevicePropertiesTTL { get; set; }
        public long DevicePropertiesRebuildTimeout { get; set; }
    }
}
