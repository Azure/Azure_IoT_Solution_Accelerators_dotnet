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
        StorageConfig MessagesConfig { get; set; }
        AlarmsConfig AlarmsConfig { get; set; }
        string StorageType { get; set; }
        Uri DocumentDbUri { get; }
        string DocumentDbKey { get; }
        int DocumentDbThroughput { get; set; }
    }

    public class ServicesConfig : IServicesConfig
    {
        public string StorageAdapterApiUrl { get; set; }

        public int StorageAdapterApiTimeout { get; set; }

        public StorageConfig MessagesConfig { get; set; }

        public AlarmsConfig AlarmsConfig { get; set; }

        public string StorageType { get; set; }

        public Uri DocumentDbUri { get; set; }

        public string DocumentDbKey { get; set; }

        public int DocumentDbThroughput { get; set; }

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
