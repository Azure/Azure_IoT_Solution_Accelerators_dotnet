// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models
{
    public class DeviceGroupConditionApiModel
    {
        [JsonProperty("Key")]
        public string Key { get; set; }

        [JsonProperty("Operator")]
        public string Operator { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}
