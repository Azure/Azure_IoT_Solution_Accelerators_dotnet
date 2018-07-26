// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers
{
    static class QueryConditionTranslator
    {
        private static readonly Dictionary<string, string> operatorMap = new Dictionary<string, string>
        {
            { "EQ", "=" },
            { "NE", "!=" },
            { "LT", "<" },
            { "LE", "<=" },
            { "GT", ">" },
            { "GE", ">=" },
            { "IN", "IN" }
        };

        public static string ToQueryString(string conditions)
        {
            IEnumerable<QueryConditionClause> clauses = null;

            try
            {
                clauses = JsonConvert.DeserializeObject<IEnumerable<QueryConditionClause>>(conditions);
            }
            catch
            {
                // Any exception raised in deserializing will be ignored
            }

            if (clauses == null)
            {
                // Condition is not a valid clause list. Assume it a query string
                return conditions;
            }

            var clauseStrings = clauses.Select(c =>
            {
                string op;
                if (!operatorMap.TryGetValue(c.Operator.ToUpperInvariant(), out op))
                {
                    throw new InvalidInputException();
                }

                // Reminder: string value will be surrounded by single quotation marks
                StringBuilder value = new StringBuilder();
                using (StringWriter sw = new StringWriter(value))
                {
                    using (JsonTextWriter writer = new JsonTextWriter(sw))
                    {
                        writer.QuoteChar = '\'';

                        JsonSerializer ser = new JsonSerializer();
                        ser.Serialize(writer, c.Value);
                    }
                }

                return $"{c.Key} {op} {value.ToString()}";
            });

            return string.Join(" and ", clauseStrings);
        }
    }
}
