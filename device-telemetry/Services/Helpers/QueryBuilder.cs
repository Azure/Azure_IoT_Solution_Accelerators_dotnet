// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers
{
    public class QueryBuilder
    {
        private const string INVALID_CHARACTER = @"[^A-Za-z0-9:;.,_\-]";

        public static SqlQuerySpec GetDocumentsSql(
            string schemaName,
            string byId,
            string byIdProperty,
            DateTimeOffset? from,
            string fromProperty,
            DateTimeOffset? to,
            string toProperty,
            string order,
            string orderProperty,
            int skip,
            int limit,
            string[] devices,
            string devicesProperty)
        {
            ValidateInput(ref schemaName);
            ValidateInput(ref fromProperty);
            ValidateInput(ref toProperty);
            ValidateInput(ref orderProperty);
            ValidateInput(ref devicesProperty);

            var sqlParameterCollection = new SqlParameterCollection();

            var queryBuilder = new StringBuilder("SELECT TOP @top * FROM c WHERE (c[\"doc.schema\"] = @schemaName");
            sqlParameterCollection.Add(new SqlParameter { Name = "@top", Value = skip + limit });
            sqlParameterCollection.Add(new SqlParameter { Name = "@schemaName", Value = schemaName });

            if (devices.Length > 0)
            {
                var devicesParameters = ConvertToSqlParameters("devicesParameterName", devices);
                queryBuilder.Append($" AND c[@devicesProperty] IN ({string.Join(",", devicesParameters.Select(p => p.Name))})");
                sqlParameterCollection.Add(new SqlParameter { Name = "@devicesProperty", Value = devicesProperty });
                foreach (var p in devicesParameters)
                {
                    sqlParameterCollection.Add(p);
                }
            }

            if (!string.IsNullOrEmpty(byId) && !string.IsNullOrEmpty(byIdProperty))
            {
                ValidateInput(ref byId);
                ValidateInput(ref byIdProperty);
                queryBuilder.Append(" AND c[@byIdProperty] = @byId");
                sqlParameterCollection.Add(new SqlParameter { Name = "@byIdProperty", Value = byIdProperty });
                sqlParameterCollection.Add(new SqlParameter { Name = "@byId", Value = byId });
            }

            if (from.HasValue)
            {
                // TODO: left operand is never null
                DateTimeOffset fromDate = from ?? default(DateTimeOffset);
                queryBuilder.Append(" AND c[@fromProperty] >= " + fromDate.ToUnixTimeMilliseconds());
                sqlParameterCollection.Add(new SqlParameter { Name = "@fromProperty", Value = fromProperty });
            }

            if (to.HasValue)
            {
                // TODO: left operand is never null
                DateTimeOffset toDate = to ?? default(DateTimeOffset);
                queryBuilder.Append(" AND c[@toProperty] <= " + toDate.ToUnixTimeMilliseconds());
                sqlParameterCollection.Add(new SqlParameter { Name = "@toProperty", Value = toProperty });
            }

            queryBuilder.Append(")");

            if (order == null || string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase))
            {
                queryBuilder.Append(" ORDER BY c[@orderProperty] DESC");
            }
            else
            {
                queryBuilder.Append(" ORDER BY c[@orderProperty] ASC");
            }
            sqlParameterCollection.Add(new SqlParameter { Name = "@orderProperty", Value = orderProperty });

            return new SqlQuerySpec(queryBuilder.ToString(), sqlParameterCollection);
        }

        public static SqlQuerySpec GetCountSql(
            string schemaName,
            string byId,
            string byIdProperty,
            DateTimeOffset? from,
            string fromProperty,
            DateTimeOffset? to,
            string toProperty,
            string[] devices,
            string devicesProperty,
            string[] filterValues,
            string filterProperty)
        {
            ValidateInput(ref schemaName);
            ValidateInput(ref fromProperty);
            ValidateInput(ref toProperty);
            ValidateInput(ref devicesProperty);
            ValidateInput(ref filterProperty);

            // build query
            // TODO - GROUPBY and DISTINCT are not supported by documentDB yet, improve query once supported
            // https://github.com/Azure/device-telemetry-dotnet/issues/58
            var queryBuilder = new StringBuilder();
            var sqlParameterCollection = new SqlParameterCollection();

            queryBuilder.Append("SELECT VALUE COUNT(1) FROM c WHERE (c[\"doc.schema\"] = @schemaName");
            sqlParameterCollection.Add(new SqlParameter { Name = "@schemaName", Value = schemaName });

            if (devices.Length > 0)
            {
                var devicesParameters = ConvertToSqlParameters("devicesParameterName", devices);
                queryBuilder.Append($" AND c[@devicesProperty] IN ({string.Join(",", devicesParameters.Select(p => p.Name))})");
                sqlParameterCollection.Add(new SqlParameter { Name = "@devicesProperty", Value = devicesProperty });
                foreach (var p in devicesParameters)
                {
                    sqlParameterCollection.Add(p);
                }
            }

            if (!string.IsNullOrEmpty(byId) && !string.IsNullOrEmpty(byIdProperty))
            {
                ValidateInput(ref byId);
                ValidateInput(ref byIdProperty);
                queryBuilder.Append(" AND c[@byIdProperty] = @byId");
                sqlParameterCollection.Add(new SqlParameter { Name = "@byIdProperty", Value = byIdProperty });
                sqlParameterCollection.Add(new SqlParameter { Name = "@byId", Value = byId });
            }

            if (from.HasValue)
            {
                // TODO: left operand is never null
                DateTimeOffset fromDate = from ?? default(DateTimeOffset);
                queryBuilder.Append(" AND c[@fromProperty] >= " + fromDate.ToUnixTimeMilliseconds());
                sqlParameterCollection.Add(new SqlParameter { Name = "@fromProperty", Value = fromProperty });
            }

            if (to.HasValue)
            {
                // TODO: left operand is never null
                DateTimeOffset toDate = to ?? default(DateTimeOffset);
                queryBuilder.Append(" AND c[@toProperty] <= " + toDate.ToUnixTimeMilliseconds());
                sqlParameterCollection.Add(new SqlParameter { Name = "@toProperty", Value = toProperty });
            }

            if (filterValues.Length > 0)
            {
                var filterParameters = ConvertToSqlParameters("filterParameterName", filterValues);
                queryBuilder.Append($" AND c[@filterProperty] IN ({string.Join(",", filterParameters.Select(p => p.Name))})");
                sqlParameterCollection.Add(new SqlParameter { Name = "@filterProperty", Value = filterProperty });
                foreach (var p in filterParameters)
                {
                    sqlParameterCollection.Add(p);
                }
            }

            queryBuilder.Append(")");

            return new SqlQuerySpec(queryBuilder.ToString(), sqlParameterCollection);
        }

        // Check illegal characters in input
        private static void ValidateInput(ref string input)
        {
            input = input.Trim();

            if (Regex.IsMatch(input, INVALID_CHARACTER))
            {
                throw new InvalidInputException($"Input '{input}' contains invalid characters.");
            }
        }

        // Convert SQL IN clause parameter into a list of SqlParameter instances naming by original name
        // and values index because Cosmos DB doesn't natively support string array as one SqlParameter.
        private static IEnumerable<SqlParameter> ConvertToSqlParameters(string name, string[] values)
        {
            return values.Select((value, index) => new SqlParameter { Name = $"@{name}{index}", Value = value });
        }
    }
}
