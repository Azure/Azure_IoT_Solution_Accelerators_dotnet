// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class Condition
    {
        public string Field { get; set; } = string.Empty;
        [JsonConverter(typeof(StringEnumConverter))]
        public OperatorType Operator { get; set; } = new OperatorType();
        public string Value { get; set; } = string.Empty;

        public Condition() { }
    }

    public enum OperatorType
    {
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Equals
    }
}
