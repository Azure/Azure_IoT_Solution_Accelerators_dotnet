// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.Auth;

// TODO: tests
// TODO: handle errors
// TODO: use binding
namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService.Runtime
{
    public interface IConfig
    {
        // Web service listening port
        int Port { get; }

        ILoggingConfig LoggingConfig { get; }

        // Service layer configuration
        IServicesConfig ServicesConfig { get; }

        // Client authentication and authorization configuration
        IClientAuthConfig ClientAuthConfig { get; }

        IBlobStorageConfig BlobStorageConfig { get; }
    }

    public class Config : IConfig
    {
        private const string APPLICATION_KEY = "AsaManagerService:";
        private const string PORT_KEY = APPLICATION_KEY + "webservice_port";

        private const string LOGGING_KEY = APPLICATION_KEY + "Logging:";
        private const string LOGGING_LOGLEVEL_KEY = LOGGING_KEY + "LogLevel";
        private const string LOGGING_INCLUDEPROCESSID_KEY = LOGGING_KEY + "IncludeProcessId";
        private const string LOGGING_DATEFORMAT_KEY = LOGGING_KEY + "DateFormat";
        private const string LOGGING_BLACKLIST_PREFIX_KEY = LOGGING_KEY + "BWListPrefix";
        private const string LOGGING_BLACKLIST_SOURCES_KEY = LOGGING_KEY + "BlackListSources";
        private const string LOGGING_WHITELIST_SOURCES_KEY = LOGGING_KEY + "WhiteListSources";
        private const string LOGGING_EXTRADIAGNOSTICS_KEY = LOGGING_KEY + "ExtraDiagnostics";
        private const string LOGGING_EXTRADIAGNOSTICSPATH_KEY = LOGGING_KEY + "ExtraDiagnosticsPath";

        private const string CLIENT_AUTH_KEY = APPLICATION_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "cors_whitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "auth_type";
        private const string AUTH_REQUIRED_KEY = CLIENT_AUTH_KEY + "auth_required";

        private const string JWT_KEY = APPLICATION_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowed_algorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "issuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "audience";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clock_skew_seconds";

        private const string EVENTHUB_KEY = APPLICATION_KEY + "EventHub:";
        private const string EVENTHUB_CONNECTION_KEY = EVENTHUB_KEY + "connection_string";
        private const string EVENTHUB_NAME = EVENTHUB_KEY + "name";
        private const string EVENTHUB_CHECKPOINT_INTERVAL_MS = EVENTHUB_KEY + "checkpoint_interval_msecs";

        private const string BLOB_STORAGE_KEY = APPLICATION_KEY + "BlobStorage:";
        private const string STORAGE_REFERENCE_DATA_CONTAINER_KEY = BLOB_STORAGE_KEY + "reference_data_container";
        private const string STORAGE_EVENTHUB_CONTAINER_KEY = BLOB_STORAGE_KEY + "eventhub_container";
        private const string STORAGE_ACCOUNT_NAME_KEY = BLOB_STORAGE_KEY + "account_name";
        private const string STORAGE_ACCOUNT_KEY_KEY = BLOB_STORAGE_KEY + "account_key";
        private const string STORAGE_ACCOUNT_ENDPOINT_KEY = BLOB_STORAGE_KEY + "account_endpoint";
        private const string STORAGE_DEVICE_GROUPS_FILE_NAME = BLOB_STORAGE_KEY + "reference_data_device_groups_file_name";
        private const string STORAGE_RULES_FILE_NAME = BLOB_STORAGE_KEY + "reference_data_rules_file_name";
        private const string STORAGE_DATE_FORMAT = BLOB_STORAGE_KEY + "reference_data_date_format";
        private const string STORAGE_TIME_FORMAT = BLOB_STORAGE_KEY + "reference_data_time_format";
        private const string STORAGE_ACCOUNT_ENDPOINT_DEFAULT = "core.windows.net";

        private const string MESSAGES_KEY = APPLICATION_KEY + "MessagesStorage:";
        private const string MESSAGES_STORAGE_TYPE_KEY = MESSAGES_KEY + "storageType";

        private const string ALARMS_KEY = APPLICATION_KEY + "AlarmsStorage:";
        private const string ALARMS_STORAGE_TYPE_KEY = ALARMS_KEY + "storageType";

        private const string RULES_KEY = "DeviceTelemetryRulesService:";
        private const string RULES_WEBSERVICE_URL_KEY = RULES_KEY + "webservice_url";
        private const string RULES_WEBSERVICE_TIMEOUT_KEY = RULES_KEY + "webservice_timeout_msecs";

        private const string CONFIG_KEY = "PCSConfigurationService:";
        private const string CONFIG_WEBSERVICE_URL_KEY = CONFIG_KEY + "webservice_url";
        private const string CONFIG_WEBSERVICE_TIMEOUT_KEY = CONFIG_KEY + "webservice_timeout_msecs";

        private const string IOTHUB_MANAGER_KEY = "IoTHubManagerService:";
        private const string IOTHUB_MANAGER_WEBSERVICE_URL_KEY = IOTHUB_MANAGER_KEY + "webservice_url";
        private const string IOTHUB_MANAGER_WEBSERVICE_TIMEOUT_KEY = IOTHUB_MANAGER_KEY + "webservice_timeout_msecs";
        private const string IOTHUB_MANAGER_RETRY_COUNT = IOTHUB_MANAGER_KEY + "retry_count";
        private const string IOTHUB_MANAGER_INITIAL_RETRY_INTERVAL_MS = IOTHUB_MANAGER_KEY + "initial_retry_interval_msecs";
        private const string IOTHUB_MANAGER_RETRY_INCREASE_FACTOR = IOTHUB_MANAGER_KEY + "retry_increase_factor";

        // Values common to all the tables (messages and alarms)
        private const string COSMOSDBSQL_CONNSTRING_KEY = "cosmosdbsql_connstring";
        private const string COSMOSDBSQL_DATABASE_KEY = "cosmosdbsql_database";
        private const string COSMOSDBSQL_CONSISTENCY_KEY = "cosmosdbsql_consistency_level";
        private const string COSMOSDBSQL_COLLECTION_KEY = "cosmosdbsql_collection";
        private const string COSMOSDBSQL_RUS_KEY = "cosmosdbsql_RUs";

        // Simple keys used internally in this class, these don't appear in the config file
        private const string MESSAGES = "messages";
        private const string ALARMS = "alarms";

        public int Port { get; }
        public ILoggingConfig LoggingConfig { get; set; }
        public IClientAuthConfig ClientAuthConfig { get; }
        public IServicesConfig ServicesConfig { get; }
        public IBlobStorageConfig BlobStorageConfig { get; }

        public Config(IConfigData configData)
        {
            this.Port = configData.GetInt(PORT_KEY);
            this.LoggingConfig = GetLogConfig(configData);
            this.ServicesConfig = GetServicesConfig(configData);
            this.ClientAuthConfig = GetClientAuthConfig(configData);
            this.BlobStorageConfig = GetBlobStorageConfig(configData);
        }

        private static ILoggingConfig GetLogConfig(IConfigData configData)
        {
            var data = configData.GetString(LOGGING_BLACKLIST_SOURCES_KEY);
            var values = data.Replace(";", ",").Replace(":", ".").Split(",");
            var blacklist = new HashSet<string>();
            foreach (var k in values) blacklist.Add(k);

            data = configData.GetString(LOGGING_WHITELIST_SOURCES_KEY);
            values = data.Replace(";", ",").Replace(":", ".").Split(",");
            var whitelist = new HashSet<string>();
            foreach (var k in values) blacklist.Add(k);

            Enum.TryParse(configData.GetString(LOGGING_LOGLEVEL_KEY, Services.Diagnostics.LoggingConfig.DEFAULT_LOGLEVEL.ToString()), true, out LogLevel logLevel);
            var result = new LoggingConfig
            {
                LogLevel = logLevel,
                BwListPrefix = configData.GetString(LOGGING_BLACKLIST_PREFIX_KEY),
                BlackList = blacklist,
                WhiteList = whitelist,
                DateFormat = configData.GetString(LOGGING_DATEFORMAT_KEY, Services.Diagnostics.LoggingConfig.DEFAULT_DATE_FORMAT),
                LogProcessId = configData.GetBool(LOGGING_INCLUDEPROCESSID_KEY, true),
                ExtraDiagnostics = configData.GetBool(LOGGING_EXTRADIAGNOSTICS_KEY, false),
                ExtraDiagnosticsPath = configData.GetString(LOGGING_EXTRADIAGNOSTICSPATH_KEY)
            };

            return result;
        }

        private static IClientAuthConfig GetClientAuthConfig(IConfigData configData)
        {
            return new ClientAuthConfig
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
        }

        private static IServicesConfig GetServicesConfig(IConfigData configData)
        {
            var messagesStorageType = GetStorageType(configData, MESSAGES);
            var alarmsStorageType = GetStorageType(configData, ALARMS);

            return new ServicesConfig
            {
                RulesWebServiceUrl = configData.GetString(RULES_WEBSERVICE_URL_KEY),
                RulesWebServiceTimeout = configData.GetInt(RULES_WEBSERVICE_TIMEOUT_KEY),
                ConfigServiceUrl = configData.GetString(CONFIG_WEBSERVICE_URL_KEY),
                ConfigServiceTimeout = configData.GetInt(CONFIG_WEBSERVICE_TIMEOUT_KEY),
                IotHubManagerServiceUrl = configData.GetString(IOTHUB_MANAGER_WEBSERVICE_URL_KEY),
                IotHubManagerServiceTimeout = configData.GetInt(IOTHUB_MANAGER_WEBSERVICE_TIMEOUT_KEY),
                IotHubManagerRetryCount = configData.GetInt(IOTHUB_MANAGER_RETRY_COUNT),
                InitialIotHubManagerRetryIntervalMs = configData.GetInt(IOTHUB_MANAGER_INITIAL_RETRY_INTERVAL_MS),
                IotHubManagerRetryIntervalIncreaseFactor = configData.GetInt(IOTHUB_MANAGER_RETRY_INCREASE_FACTOR),
                MessagesStorageType = messagesStorageType,
                MessagesCosmosDbConfig = GetAsaOutputStorageConfig(configData, MESSAGES, messagesStorageType),
                AlarmsStorageType = alarmsStorageType,
                AlarmsCosmosDbConfig = GetAsaOutputStorageConfig(configData, ALARMS, alarmsStorageType),
                EventHubConnectionString = configData.GetString(EVENTHUB_CONNECTION_KEY),
                EventHubName = configData.GetString(EVENTHUB_NAME),
                EventHubCheckpointTimeMs = configData.GetInt(EVENTHUB_CHECKPOINT_INTERVAL_MS)
            };
        }

        private static IBlobStorageConfig GetBlobStorageConfig(IConfigData configData)
        {
            return new BlobStorageConfig
            {
                ReferenceDataContainer = configData.GetString(STORAGE_REFERENCE_DATA_CONTAINER_KEY),
                EventHubContainer = configData.GetString(STORAGE_EVENTHUB_CONTAINER_KEY),
                AccountKey = configData.GetString(STORAGE_ACCOUNT_KEY_KEY),
                AccountName = configData.GetString(STORAGE_ACCOUNT_NAME_KEY),
                EndpointSuffix = configData.GetString(STORAGE_ACCOUNT_ENDPOINT_KEY, STORAGE_ACCOUNT_ENDPOINT_DEFAULT),
                ReferenceDataDeviceGroupsFileName = configData.GetString(STORAGE_DEVICE_GROUPS_FILE_NAME),
                ReferenceDataRulesFileName = configData.GetString(STORAGE_RULES_FILE_NAME),
                ReferenceDataDateFormat = configData.GetString(STORAGE_DATE_FORMAT),
                ReferenceDataTimeFormat = configData.GetString(STORAGE_TIME_FORMAT)
            };
        }

        /// <summary>Detect the table type. The allowed values are defined by AsaOutputStorageType enum</summary>
        private static AsaOutputStorageType GetStorageType(IConfigData configData, string tableName)
        {
            AsaOutputStorageType result;

            switch (tableName)
            {
                case MESSAGES:
                    if (!Enum.TryParse(configData.GetString(MESSAGES_STORAGE_TYPE_KEY), true, out result))
                    {
                        result = AsaOutputStorageType.CosmosDbSql;
                    }

                    break;

                case ALARMS:
                    if (!Enum.TryParse(configData.GetString(ALARMS_STORAGE_TYPE_KEY), true, out result))
                    {
                        result = AsaOutputStorageType.CosmosDbSql;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unknown table name `{tableName}`");
            }

            return result;
        }

        /// <summary>Read storage configuration, depending on the storage type</summary>
        /// <param name="configData">Raw configuration data</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="storageType">Type of storage, e.g. CosmosDbSql</param>
        private static CosmosDbTableConfiguration GetAsaOutputStorageConfig(
            IConfigData configData,
            string tableName,
            AsaOutputStorageType storageType)
        {
            string prefix;

            // All tables have the same configuration block, with a different prefix
            switch (tableName)
            {
                case MESSAGES:
                    prefix = MESSAGES_KEY;
                    break;
                case ALARMS:
                    prefix = ALARMS_KEY;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown table " + tableName);
            }

            // If the table is on CosmosDb, read the configuration parameters
            if (storageType == AsaOutputStorageType.CosmosDbSql)
            {
                var consistency = configData.GetString(prefix + COSMOSDBSQL_CONSISTENCY_KEY);
                if (!Enum.TryParse<ConsistencyLevel>(consistency, true, out var consistencyLevel))
                {
                    consistencyLevel = ConsistencyLevel.Eventual;
                }

                return new CosmosDbTableConfiguration
                {
                    Api = CosmosDbApi.Sql,
                    ConnectionString = configData.GetString(prefix + COSMOSDBSQL_CONNSTRING_KEY),
                    Database = configData.GetString(prefix + COSMOSDBSQL_DATABASE_KEY),
                    Collection = configData.GetString(prefix + COSMOSDBSQL_COLLECTION_KEY),
                    ConsistencyLevel = consistencyLevel,
                    RUs = configData.GetInt(prefix + COSMOSDBSQL_RUS_KEY),
                };
            }

            // If another storage type is added, add a similar block here,
            // and change the output type to allow multiple types. Not needed now.

            return null;
        }
    }
}
