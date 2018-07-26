// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers
{
    class RuleTemplateValidator
    {
        public static bool IsValid(string pathToTemplate)
        {
            // TODO: JsonSchema is obsolete - see http://www.newtonsoft.com/jsonschema for more details
            var fileSchema = JsonSchema.Parse(
                @"{
                'type':'object',
                'properties':
                {
                    'ETag':{'required':false,'type':['string','null']},
                    'Id':{'required':false,'type':['string','null']},
                    'Name':{'required':true,'type':['string']},
                    'DateCreated':{'required':false,'type':['string','null']},
                    'DateModified':{'required':false,'type':['string','null']},
                    'Enabled':{'required':true},
                    'Description':{'required':true,'type':['string']},
                    'GroupId':{'required':true,'type':['string']},
                    'Severity':{'required':true,'type':['string']},
                    'Calculation':{'required':true,'type':['string']},
                    'TimePeriod':{'required':true,'type':['string']},
                    'Conditions':
                    {
                        'required':true,
                        'type':['array'],
                        'items':
                        {
                            'type':['object','null'],
                            'properties':
                            {
                                'Field':{'required':true,'type':['string','null']},
                                'Operator':{'required':true,'type':['string','null']},
                                'Value':{'required':true,'type':['string','null']}
                            }
                        }
                    }
                }
            }");

            if (!File.Exists(pathToTemplate))
            {
                throw new InvalidConfigurationException(
                    "Cannot open rules template file " + pathToTemplate);
            }

            JToken json = JToken.Parse(File.ReadAllText(pathToTemplate));

            JArray rulesList = (JArray) json["Rules"];

            if (rulesList == null) return false;

            foreach (var rule in rulesList)
            {
                // TODO: IsValid is obsolete - see http://www.newtonsoft.com/jsonschema for more details
                if (!rule.IsValid(fileSchema)) return false;
            }

            return true;
        }
    }
}
