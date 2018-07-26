// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers
{
    internal class IoTHubConnectionHelper
    {
        /// <summary>
        /// ensure throw friendly exception if hubConnString is not valid
        /// </summary>
        /// <param name="hubConnString">The iotHub connectionString</param>
        /// <param name="action">The create action using iotHub connectionString</param>
        public static void CreateUsingHubConnectionString(string hubConnString, Action<string> action)
        {
            try
            {
                action(hubConnString);
            }
            catch (ArgumentException argumentException)
            {
                // Format is not correct, for example: missing hostname
                throw new InvalidConfigurationException($"Invalid service configuration for HubConnectionString. Exception details: {argumentException.Message}");
            }
            catch (FormatException formatException)
            {
                // SharedAccessKey is not valid base-64 string
                throw new InvalidConfigurationException($"Invalid service configuration for HubConnectionString: {hubConnString}. Exception details: {formatException.Message}");
            }
        }
    }
}
