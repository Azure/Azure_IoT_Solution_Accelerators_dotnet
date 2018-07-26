// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime
{
    public interface IBlobStorageConfig
    {
        // Azure blob credentials
        string AccountName { get; }
        string AccountKey { get; }
        string EndpointSuffix { get; }
        
        // TODO: add docs
        string EventHubContainer { get; set; }
        
        // Azure blob where the reference data is uploaded
        string ReferenceDataContainer { get; }
        
        // Date format in ASA path pattern
        string ReferenceDataDateFormat { get; }
        
        // Time format in ASA path pattern
        string ReferenceDataTimeFormat { get; }
        
        // Name of the file storing the device groups map
        string ReferenceDataDeviceGroupsFileName { get; }
        
        // Name of the file storing the rules
        string ReferenceDataRulesFileName { get; set; }
    }

    public class BlobStorageConfig : IBlobStorageConfig
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string EndpointSuffix { get; set; }

        public string EventHubContainer { get; set; }
        
        public string ReferenceDataContainer { get; set; }
        public string ReferenceDataDateFormat { get; set; }
        public string ReferenceDataTimeFormat { get; set; }
        public string ReferenceDataDeviceGroupsFileName { get; set; }
        public string ReferenceDataRulesFileName { get; set; }
    }
}
