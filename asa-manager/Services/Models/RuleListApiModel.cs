// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Models
{
    // see https://github.com/Azure/device-telemetry-dotnet/blob/master/WebService/v1/Models/RuleListApiModel.cs
    public class RuleListApiModel
    {
        [JsonProperty("Items")]
        public IEnumerable<RuleApiModel> Items { get; set; }

        public RuleListApiModel()
        {
            this.Items = new List<RuleApiModel>();
        }
    }
}
