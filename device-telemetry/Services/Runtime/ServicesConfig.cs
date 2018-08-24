// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime
{
    public interface IServicesConfig
    {
        string StorageAdapterApiUrl { get; set; }
        int StorageAdapterApiTimeout { get; set; }
        string UserManagementApiUrl { get; }
        StorageConfig MessagesConfig { get; set; }
        AlarmsConfig AlarmsConfig { get; set; }
        string StorageType { get; set; }
        Uri DocumentDbUri { get; }
        string DocumentDbKey { get; }
        int DocumentDbThroughput { get; set; }
        string EventHubName { get; set; }
        string EventHubConnectionString { get; set; }
        int EventHubOffsetTimeInMinutes { get; set; }
        string LogicAppEndPointUrl { get; set; }
        string SolutionName { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageAdapterApiUrl { get; set; }

        public int StorageAdapterApiTimeout { get; set; }

        public string UserManagementApiUrl { get; set; }

        public StorageConfig MessagesConfig { get; set; }

        public AlarmsConfig AlarmsConfig { get; set; }

        public string StorageType { get; set; }

        public Uri DocumentDbUri { get; set; }

        public string DocumentDbKey { get; set; }

        public string EventHubConnectionString { get; set; }

        public string EventHubName { get; set; }

        public int EventHubOffsetTimeInMinutes { get; set; }

        public int DocumentDbThroughput { get; set; }

        public string LogicAppEndPointUrl { get; set; }

        public string SolutionName { get; set; }

        public string DocumentDbConnString
        {
            set
            {
                var match = Regex.Match(value,
                    @"^AccountEndpoint=(?<endpoint>.*);AccountKey=(?<key>.*);$");

                Uri endpoint;

                if (!match.Success ||
                    !Uri.TryCreate(match.Groups["endpoint"].Value,
                        UriKind.RelativeOrAbsolute,
                        out endpoint))
                {
                    var message = "Invalid connection string for DocumentDB";
                    throw new InvalidConfigurationException(message);
                }

                this.DocumentDbUri = endpoint;
                this.DocumentDbKey = match.Groups["key"].Value;
            }
        }
    }
}
