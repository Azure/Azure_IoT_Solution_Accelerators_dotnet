// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    /// <summary>
    /// Class to enclose all the details of an action. 
    /// Action Type is an enum which is modified when a new ActionType Implementation is added. 
    /// Parameters is a Dictionary used to store all the other related info required for an action type.
    /// </summary>
    public interface IActionItem
    {
        [JsonConverter(typeof(StringEnumConverter))]
        Type Type { get; set; }

        IDictionary<string, object> Parameters { get; set; }
    }

    public class EmailActionItem : IActionItem
    {
        private const string SUBJECT = "Subject";
        private const string TEMPLATE = "Template";
        private const string EMAIL = "Email";

        [JsonConverter(typeof(StringEnumConverter))]
        public Type Type { get; set; }

        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public EmailActionItem() { }

        public EmailActionItem(Type type, IDictionary<string, object> parameters)
        {
            parameters = new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);
            this.Type = type;

            if (parameters.ContainsKey(TEMPLATE)) this.Parameters[TEMPLATE] = parameters[TEMPLATE];
            if (parameters.ContainsKey(SUBJECT)) this.Parameters[SUBJECT] = parameters[SUBJECT];
            try
            {
                if (parameters.ContainsKey(EMAIL))
                {
                    this.Parameters[EMAIL] = ((Newtonsoft.Json.Linq.JArray)parameters[EMAIL]).ToObject<List<String>>();
                }
                else
                {
                    throw new InvalidInputException("No Email address provided for actionType Email");
                }
            }
            catch (InvalidCastException e)
            {
                throw new InvalidInputException("Email field is a list of string");
            }

            if (!this.IsValid())
            {
                throw new InvalidInputException("Invalid Email Address.");
            }
        }

        private bool IsValid()
        {
            var emailList = (List<string>)this.Parameters[EMAIL];
            try
            {
                if (!emailList.Any()) throw new InvalidInputException("Empty email list provided for actionType Email");
                foreach (string emailToValidate in emailList)
                {
                    MailAddress mail = new MailAddress(emailToValidate);
                }
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public enum Type
    {
        Email
    }
}
