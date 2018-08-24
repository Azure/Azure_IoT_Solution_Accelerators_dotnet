// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ActionApiModel
    {

        private delegate IActionItem ActionItemMap(Services.Models.Type type, IDictionary<string, object> param);

        [JsonProperty(PropertyName = "Type")]
        public string ActionType { get; set; } = string.Empty;

        // Parameters dictionary is case-insensitive.
        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        // Maps Types of actions to their Service layer class.
        private static IDictionary<Services.Models.Type, ActionItemMap> validationMapping = new Dictionary<Services.Models.Type, ActionItemMap>()
        {
            {Services.Models.Type.Email, (Services.Models.Type type, IDictionary<string, object> param) => { return new EmailActionItem(type, param); } }
        };

        public ActionApiModel(string action, Dictionary<string, object> parameters)
        {
            this.ActionType = action;
            this.Parameters = parameters;
        }

        public ActionApiModel(Services.Models.IActionItem action)
        {
            this.ActionType = action.Type.ToString();
            this.Parameters = action.Parameters;
        }

        public ActionApiModel() { }

        public IActionItem ToServiceModel()
        {
            if (!Enum.TryParse<Services.Models.Type>(this.ActionType, true, out Services.Models.Type action))
            {
                throw new InvalidInputException($"The action type {this.ActionType} is not valid");
            }
            return validationMapping[action](action, this.Parameters);
        }
    }
}
