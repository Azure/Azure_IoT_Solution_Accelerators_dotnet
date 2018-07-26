// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers
{
    internal class QueryConditionClause
    {
        /// <summary>
        /// The full name of tag, desired or reported properties, in form like "tags.Building" or "reported.System.Processor"
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The compare operator. Currently, it could be one of values below:
        /// EQ, NE, LT, LE, GT, GE, IN
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// The value to be compared
        /// Reminder: value could be in type number, string and boolean
        /// </summary>
        public object Value { get; set; }
    }
}
