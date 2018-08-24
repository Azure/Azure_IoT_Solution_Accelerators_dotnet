// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models
{
    public interface IActionAsaModel
    {
        string ActionType { get; set; }
        IDictionary<string, object> Parameters { get; set; }
    }

    public class ActionAsaModel : IActionAsaModel
    {
        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = string.Empty;

        // Parameters dictionary is case-insensitive.
        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public ActionAsaModel() { }
    }
}
