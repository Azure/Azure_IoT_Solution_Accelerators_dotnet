// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.External
{
    public class ValueListApiModel
    {
        public IEnumerable<ValueApiModel> Items;

        [JsonProperty("$metadata")]
        public Dictionary<string, string> Metadata;
    }
}
