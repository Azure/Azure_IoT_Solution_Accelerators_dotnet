// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Auth;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Runtime
{
    public interface IConfig
    {
        // Web service listening port
        int Port { get; }

        // Service layer configuration
        IServicesConfig ServicesConfig { get; }

        // Client authentication and authorization configuration
        IClientAuthConfig ClientAuthConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string APPLICATION_KEY = "IothubManagerService:";
        private const string PORT_KEY = APPLICATION_KEY + "webservice_port";
        private const string IOTHUB_CONNSTRING_KEY = APPLICATION_KEY + "iothub_connstring";
        private const string DEVICE_PROPERTIES_KEY = APPLICATION_KEY + "DevicePropertiesCache:"; 
        private const string DEVICE_PROPERTIES_WHITELIST_KEY = DEVICE_PROPERTIES_KEY + "whitelist";
        private const string DEVICE_PROPERTIES_TTL_KEY = DEVICE_PROPERTIES_KEY + "TTL";
        private const string DEVICE_PROPERTIES_REBUILD_TIMEOUT_KEY = DEVICE_PROPERTIES_KEY + "rebuild_timeout";

        private const string STORAGE_ADAPTER_KEY = "StorageAdapterService:";
        private const string STORAGE_ADAPTER_URL_KEY = STORAGE_ADAPTER_KEY + "webservice_url";

        private const string CLIENT_AUTH_KEY = APPLICATION_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "cors_whitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "auth_type";
        private const string AUTH_REQUIRED_KEY = CLIENT_AUTH_KEY + "auth_required";

        private const string JWT_KEY = APPLICATION_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowed_algorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "issuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "audience";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clock_skew_seconds";

        public int Port { get; }
        public IServicesConfig ServicesConfig { get; }
        public IClientAuthConfig ClientAuthConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);

            var connstring = configData.GetString(IOTHUB_CONNSTRING_KEY);
            if (connstring.ToLowerInvariant().Contains("your azure iot hub"))
            {
                // In order to connect to Azure IoT Hub, the service requires a connection
                // string. The value can be found in the Azure Portal. For more information see
                // https://docs.microsoft.com/azure/iot-hub/iot-hub-csharp-csharp-getstarted
                // to find the connection string value.
                // The connection string can be stored in the 'appsettings.ini' configuration
                // file, or in the PCS_IOTHUB_CONNSTRING environment variable. When
                // working with VisualStudio, the environment variable can be set in the
                // WebService project settings, under the "Debug" tab.
                throw new Exception("The service configuration is incomplete. " +
                                    "Please provide your Azure IoT Hub connection string. " +
                                    "For more information, see the environment variables " +
                                    "used in project properties and the 'iothub_connstring' " +
                                    "value in the 'appsettings.ini' configuration file.");
            }

            this.ServicesConfig = new ServicesConfig
            {
                IoTHubConnString = configData.GetString(IOTHUB_CONNSTRING_KEY),
                DevicePropertiesWhiteList = configData.GetString(DEVICE_PROPERTIES_WHITELIST_KEY),
                DevicePropertiesTTL = configData.GetInt(DEVICE_PROPERTIES_TTL_KEY),
                DevicePropertiesRebuildTimeout = configData.GetInt(DEVICE_PROPERTIES_REBUILD_TIMEOUT_KEY),
                StorageAdapterApiUrl = configData.GetString(STORAGE_ADAPTER_URL_KEY),
            };

            this.ClientAuthConfig = new ClientAuthConfig
            {
                // By default CORS is disabled
                CorsWhitelist = configData.GetString(CORS_WHITELIST_KEY, string.Empty),
                // By default Auth is required
                AuthRequired = configData.GetBool(AUTH_REQUIRED_KEY, true),
                // By default auth type is JWT
                AuthType = configData.GetString(AUTH_TYPE_KEY, "JWT"),
                // By default the only trusted algorithms are RS256, RS384, RS512
                JwtAllowedAlgos = configData.GetString(JWT_ALGOS_KEY, "RS256,RS384,RS512").Split(','),
                JwtIssuer = configData.GetString(JWT_ISSUER_KEY),
                JwtAudience = configData.GetString(JWT_AUDIENCE_KEY),
                // By default the allowed clock skew is 2 minutes
                JwtClockSkew = TimeSpan.FromSeconds(configData.GetInt(JWT_CLOCK_SKEW_KEY, 120)),
            };
        }
    }
}
