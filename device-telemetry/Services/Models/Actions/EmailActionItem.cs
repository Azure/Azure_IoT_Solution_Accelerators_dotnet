// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models.Actions
{
    public class EmailActionItem : IActionItem
    {
        private const string SUBJECT = "Subject";
        private const string TEMPLATE = "Template";
        private const string EMAIL = "Email";

        [JsonConverter(typeof(StringEnumConverter))]
        public ActionType ActionType { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

        public EmailActionItem()
        {
            this.ActionType = ActionType.Email;
            this.Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public EmailActionItem(IDictionary<string, object> parameters)
        {
            this.ActionType = ActionType.Email;
            this.Parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // convert input to case-insensitive dictionary
            parameters = new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);

            if (!(parameters.ContainsKey(TEMPLATE) &&
                  parameters.ContainsKey(SUBJECT) &&
                  parameters.ContainsKey(EMAIL)))
            {
                throw new InvalidInputException("Error, missing parameter for email action. " +
                                                $"Required fields are: {SUBJECT}, {TEMPLATE}, and {EMAIL}.");
            }

            if (parameters.ContainsKey(TEMPLATE))
            {
                this.Parameters[TEMPLATE] = parameters[TEMPLATE];
            }

            if (parameters.ContainsKey(SUBJECT))
            {
                this.Parameters[SUBJECT] = parameters[SUBJECT];
            }

            if (parameters.ContainsKey(EMAIL))
            {
                this.Parameters[EMAIL] = this.ValidateAndConvertEmails(parameters[EMAIL]);
            }
        }

        /// <summary>
        /// Validates email address list and converts to a list
        /// </summary>
        private List<string> ValidateAndConvertEmails(Object emails)
        {
            List<string> result = new List<string>();

            try
            {
                result = ((JArray)emails).ToObject<List<string>>();
            }
            catch (Exception e)
            {
                var msg = "Emails provided should be an array of valid email addresses as strings.";
                throw new InvalidInputException(msg, e);
            }

            if (!result.Any())
            {
                throw new InvalidInputException("Error, email list for action ActionType Email is empty. " +
                                                "Must provide at least one valid email address.");
            }

            foreach (var email in result)
            {
                try
                {
                    var address = new MailAddress(email);
                }
                catch (Exception e)
                {
                    throw new InvalidInputException("Error, with email format. Invlaid email provided " +
                              "for email Action. Must provide at least one valid email address.");
                }
                
            }

            return result;
        }
    }
}
