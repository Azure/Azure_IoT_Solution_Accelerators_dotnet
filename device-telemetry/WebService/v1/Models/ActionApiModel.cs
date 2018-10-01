﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class ActionApiModel
    {
        [JsonProperty(PropertyName = "ActionType")]
        public string ActionType { get; set; } = string.Empty;

        // Note: Parameters dictionary should always be initialized as case-insensitive.
        [JsonProperty(PropertyName = "Parameters")]
        public IDictionary<string, object> Parameters { get; set; }

        public ActionApiModel(string action, Dictionary<string, object> parameters)
        {
            this.ActionType = action;

            try
            {
                this.Parameters = new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                var msg = $"Error, duplicate parameters provided for the {this.ActionType} action. " +
                          "Parameters are case-insensitive.";
                throw new InvalidInputException(msg, e);
            }
        }

        public ActionApiModel(IActionItem action)
        {
            this.ActionType = action.ActionType.ToString();
            this.Parameters = action.Parameters;
        }

        public ActionApiModel()
        {
            this.ActionType = Services.Models.Actions.ActionType.Email.ToString();
            this.Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public IActionItem ToServiceModel()
        {
            if (!Enum.TryParse(this.ActionType, true, out ActionType action))
            {
                var validActionsList = string.Join(", ", Enum.GetNames(typeof(ActionType)).ToList());
                throw new InvalidInputException($"The action type '{this.ActionType}' is not valid." +
                                                $"Valid action types: [{validActionsList}]");
            }

            switch (action)
            {
                case Services.Models.Actions.ActionType.Email:
                    return new EmailActionItem(this.Parameters);
                default:
                    var validActionsList = string.Join(", ", Enum.GetNames(typeof(ActionType)).ToList());
                    throw new InvalidInputException($"The action type '{this.ActionType}' is not valid" +
                                                    $"Valid action types: [{validActionsList}]");
            }
        }
    }
}
