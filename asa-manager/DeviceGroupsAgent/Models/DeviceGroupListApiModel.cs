// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models
{
    public class DeviceGroupListApiModel
    {
        [JsonProperty("Items")]
        public IEnumerable<DeviceGroupApiModel> Items { get; set; }
    }
}
