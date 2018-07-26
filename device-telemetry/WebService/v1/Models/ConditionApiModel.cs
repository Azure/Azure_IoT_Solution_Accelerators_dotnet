// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;
using System;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ConditionApiModel
    {
        [JsonProperty(PropertyName = "Field")]
        public string Field { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Operator")]
        public string Operator { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; } = string.Empty;

        public ConditionApiModel() { }

        public ConditionApiModel(Condition condition)
        {
            if (condition != null)
            {
                this.Field = condition.Field;
                this.Operator = condition.Operator.ToString();
                this.Value = condition.Value;
            }
        }

        public Condition ToServiceModel()
        {
            OperatorType operatorInstance = new OperatorType();
            if (!Enum.TryParse<OperatorType>(this.Operator, true, out operatorInstance))
            {
                throw new InvalidInputException("The value of 'Operator' is not valid");
            }
            return new Condition()
            {
                Field = this.Field,
                Operator = operatorInstance,
                Value = this.Value
            };
        }
    }
}
