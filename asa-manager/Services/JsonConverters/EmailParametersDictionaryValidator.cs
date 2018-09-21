// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.JsonConverters
{
    public class EmailParametersDictionaryValidator : JsonConverter
    {
        private const string EMAIL_KEY = "Email";
        private const string TEMPLATE_KEY = "Template";
        private const string SUBJECT_KEY = "Subject";

        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var returnDictionary = new Dictionary<string, object>();
            JObject jsonObject = JObject.Load(reader);
            // Casting to proper types.
            // Converting this to a case-insensitive dictionary for case insensitive look up.
            Dictionary<string, object> caseInsensitiveJsonDictionary = new Dictionary<string, object>(jsonObject.ToObject<Dictionary<string, object>>(), StringComparer.OrdinalIgnoreCase);
            if (caseInsensitiveJsonDictionary.ContainsKey("Email") && caseInsensitiveJsonDictionary["Email"] != null)
                returnDictionary["Email"] = ((JArray)caseInsensitiveJsonDictionary["Email"]).ToObject<List<string>>();
            if (caseInsensitiveJsonDictionary.ContainsKey("Template") && caseInsensitiveJsonDictionary["Template"] != null)
                returnDictionary["Template"] = caseInsensitiveJsonDictionary["Template"];
            if (caseInsensitiveJsonDictionary.ContainsKey("Subject") && caseInsensitiveJsonDictionary["Subject"] != null)
                returnDictionary["Subject"] = caseInsensitiveJsonDictionary["Subject"];
            return returnDictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Use default implementation for writing to the field.");
        }
    }
}
