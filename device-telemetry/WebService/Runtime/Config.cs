// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.Auth;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.Runtime
{
    public interface IConfig
    {
        // Web service listening port
        int Port { get; }

        // Service layer configuration
        IServicesConfig ServicesConfig { get; }

        // Client authentication and authorization configuration
        IClientAuthConfig ClientAuthConfig { get; }

        IBlobStorageConfig BlobStorageConfig { get; }
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string APPLICATION_KEY = "TelemetryService:";
        private const string PORT_KEY = APPLICATION_KEY + "webservice_port";
        private const string STORAGE_TYPE_KEY = APPLICATION_KEY + "storage_type";
        private const string SOLUTION_NAME_KEY = APPLICATION_KEY + "solution_name";

        private const string DOCUMENTDB_KEY = "TelemetryService:DocumentDb:";
        private const string DOCUMENTDB_CONNSTRING_KEY = DOCUMENTDB_KEY + "connstring";
        private const string DOCUMENTDB_RUS_KEY = DOCUMENTDB_KEY + "RUs";

        private const string MESSAGES_DB_KEY = "TelemetryService:Messages:";
        private const string MESSAGES_DB_DATABASE_KEY = MESSAGES_DB_KEY + "database";
        private const string MESSAGES_DB_COLLECTION_KEY = MESSAGES_DB_KEY + "collection";

        private const string ALARMS_DB_KEY = "TelemetryService:Alarms:";
        private const string ALARMS_DB_DATABASE_KEY = ALARMS_DB_KEY + "database";
        private const string ALARMS_DB_COLLECTION_KEY = ALARMS_DB_KEY + "collection";
        private const string ALARMS_DB_MAX_DELETE_RETRIES = ALARMS_DB_KEY + "max_delete_retries";

        private const string STORAGE_ADAPTER_KEY = "StorageAdapterService:";
        private const string STORAGE_ADAPTER_API_URL_KEY = STORAGE_ADAPTER_KEY + "webservice_url";
        private const string STORAGE_ADAPTER_API_TIMEOUT_KEY = STORAGE_ADAPTER_KEY + "webservice_timeout";

        private const string USER_MANAGEMENT_KEY = "UserManagementService:";
        private const string USER_MANAGEMENT_URL_KEY = USER_MANAGEMENT_KEY + "webservice_url";

        private const string CLIENT_AUTH_KEY = APPLICATION_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "cors_whitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "auth_type";
        private const string AUTH_REQUIRED_KEY = CLIENT_AUTH_KEY + "auth_required";

        private const string JWT_KEY = APPLICATION_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowed_algorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "issuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "audience";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clock_skew_seconds";

        private const string BLOB_STORAGE_KEY = APPLICATION_KEY + "BlobStorage:";
        private const string STORAGE_EVENTHUB_CONTAINER_KEY = BLOB_STORAGE_KEY + "eventhub_container";
        private const string STORAGE_ACCOUNT_NAME_KEY = BLOB_STORAGE_KEY + "account_name";
        private const string STORAGE_ACCOUNT_KEY_KEY = BLOB_STORAGE_KEY + "account_key";
        private const string STORAGE_ACCOUNT_ENDPOINT_KEY = BLOB_STORAGE_KEY + "account_endpoint";
        private const string STORAGE_ACCOUNT_ENDPOINT_DEFAULT = "core.windows.net";

        private const string EVENTHUB_KEY = APPLICATION_KEY + "EventHub:";
        private const string EVENTHUB_CONNECTION_KEY = EVENTHUB_KEY + "connection_string";
        private const string EVENTHUB_NAME = EVENTHUB_KEY + "name";

        private const string LOGICAPP_KEY = APPLICATION_KEY + "LogicApp:";
        private const string LOGICAPP_ENDPOINT_URL = LOGICAPP_KEY + "endpoint_url";

        public int Port { get; }
        public IServicesConfig ServicesConfig { get; }
        public IClientAuthConfig ClientAuthConfig { get; }
        public IBlobStorageConfig BlobStorageConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);

            this.ServicesConfig = new ServicesConfig
            {
                MessagesConfig = new StorageConfig(
                    configData.GetString(MESSAGES_DB_DATABASE_KEY),
                    configData.GetString(MESSAGES_DB_COLLECTION_KEY)),
                AlarmsConfig = new AlarmsConfig(
                    configData.GetString(ALARMS_DB_DATABASE_KEY),
                    configData.GetString(ALARMS_DB_COLLECTION_KEY),
                    configData.GetInt(ALARMS_DB_MAX_DELETE_RETRIES)),
                StorageType = configData.GetString(STORAGE_TYPE_KEY),
                DocumentDbConnString = configData.GetString(DOCUMENTDB_CONNSTRING_KEY),
                DocumentDbThroughput = configData.GetInt(DOCUMENTDB_RUS_KEY),
                StorageAdapterApiUrl = configData.GetString(STORAGE_ADAPTER_API_URL_KEY),
                StorageAdapterApiTimeout = configData.GetInt(STORAGE_ADAPTER_API_TIMEOUT_KEY),
                EventHubConnectionString = configData.GetString(EVENTHUB_CONNECTION_KEY),
                EventHubName = configData.GetString(EVENTHUB_NAME),
                LogicAppEndPointUrl = configData.GetString(LOGICAPP_ENDPOINT_URL),
                SolutionName = configData.GetString(SOLUTION_NAME_KEY),
                UserManagementApiUrl = configData.GetString(USER_MANAGEMENT_URL_KEY)
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
                JwtIssuer = configData.GetString(JWT_ISSUER_KEY, String.Empty),
                JwtAudience = configData.GetString(JWT_AUDIENCE_KEY, String.Empty),
                // By default the allowed clock skew is 2 minutes
                JwtClockSkew = TimeSpan.FromSeconds(configData.GetInt(JWT_CLOCK_SKEW_KEY, 120)),
            };

            this.BlobStorageConfig = new BlobStorageConfig
            {
                AccountKey = configData.GetString(STORAGE_ACCOUNT_KEY_KEY),
                AccountName = configData.GetString(STORAGE_ACCOUNT_NAME_KEY),
                EndpointSuffix = configData.GetString(STORAGE_ACCOUNT_ENDPOINT_KEY, STORAGE_ACCOUNT_ENDPOINT_DEFAULT),
                EventHubContainer = configData.GetString(STORAGE_EVENTHUB_CONTAINER_KEY)
            };
        }
    }
}
