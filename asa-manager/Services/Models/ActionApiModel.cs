// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.JsonConverters;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Models
{
    public interface IActionApiModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        Type ActionType { get; set; }
        IDictionary<string, Object> Parameters { get; set; }
    }

    public class EmailActionApiModel : IActionApiModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Type ActionType { get; set; }

        // Parameters dictionary is case-insensitive.
        [JsonConverter(typeof(EmailParametersDictionaryValidator))]
        public IDictionary<string, Object> Parameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public EmailActionApiModel() { }

        public override bool Equals(object obj)
        {
            if (!(obj is EmailActionApiModel x))
            {
                return false;
            }
            else
            {
                obj = (EmailActionApiModel)obj;
            }
            return this.ActionType.Equals(x.ActionType)
                && this.IsEqualDictionary(x.Parameters);
        }

        public override int GetHashCode()
        {
            var hashCode = (this.ActionType != null ? this.ActionType.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (this.Parameters != null ? this.Parameters.GetHashCode() : 0);
            return hashCode;
        }

        // Checks if both the dictionaries have the same keys and values.
        // For a dictionary[key] => list, does a comaprison of all the elements of the list, regardless of order. 
        private bool IsEqualDictionary(IDictionary<string, object> compareDictionary)
        {
            /*
            Possible cases: 
            1. Both empty.
            2. One is empty.
            3. Different number of key value pairs.
            4. Same key, different value of type string.
            5. Same key, different value of type list.
            6. Same key, same value in different order for list.
            7. Same key, same value in same order.
            8. Same key, one is string, one is list.
            */
            if (this.Parameters.Count != compareDictionary.Count) return false;

            foreach (var key in this.Parameters.Keys)
            {
                if (!compareDictionary.ContainsKey(key) ||
                    !IsSameType(this.Parameters[key], compareDictionary[key]))
                {
                    return false;
                }
                else if (this.Parameters[key] is IList<string> &&
                    !IsListEqual((List<string>)this.Parameters[key], (List<string>)compareDictionary[key]))
                {
                    return false;
                }
                else if (!(this.Parameters[key] is IList<string>) &&
                    !compareDictionary[key].Equals(this.Parameters[key]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsListEqual(List<string> list1, List<string> list2)
        {
            return list1.Count == list2.Count && !list1.Except(list2).Any();
        }

        private static bool IsSameType(object a, object b)
        {
            // Checks if two objects are of same type
            return a.GetType() == b.GetType();
        }
    }

    public enum Type
    {
        Email
    }
}
