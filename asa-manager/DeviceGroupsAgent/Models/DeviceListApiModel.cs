// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models
{
    public class DeviceListApiModel
    {
        [JsonProperty("Items")]
        public IEnumerable<DeviceApiModel> Items { get; set; }

        public string ContinuationToken { get; set; }
    }
}
