// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
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
    }

    /// <summary>Web service configuration</summary>
    public class Config : IConfig
    {
        private const string APPLICATION_KEY = "TelemetryService:";
        private const string PORT_KEY = APPLICATION_KEY + "webservicePort";

        private const string COSMOSDB_KEY = "TelemetryService:CosmosDb:";
        private const string COSMOSDB_CONNSTRING_KEY = COSMOSDB_KEY + "documentDBConnectionString";
        private const string COSMOSDB_RUS_KEY = COSMOSDB_KEY + "RUs";

        private const string TIME_SERIES_KEY = APPLICATION_KEY + "TimeSeries:";
        private const string TIME_SERIES_FQDN = TIME_SERIES_KEY + "tsiDataAccessFQDN";
        private const string TIME_SERIES_AUTHORITY = TIME_SERIES_KEY + "authority";
        private const string TIME_SERIES_AUDIENCE = TIME_SERIES_KEY + "audience";
        private const string TIME_SERIES_EXPLORER_URL = TIME_SERIES_KEY + "explorerUrl";
        private const string TIME_SERIES_API_VERSION = TIME_SERIES_KEY + "apiVersion";
        private const string TIME_SERIES_TIMEOUT = TIME_SERIES_KEY + "timeout";

        private const string AAD_KEY = APPLICATION_KEY + "AzureActiveDirectory:";
        private const string AAD_TENANT = AAD_KEY + "aadTenantId";
        private const string AAD_APP_ID = AAD_KEY + "aadAppId";
        private const string AAD_APP_SECRET = AAD_KEY + "aadAppSecret";

        private const string MESSAGES_DB_KEY = "TelemetryService:Messages:";
        private const string MESSAGES_DB_DATABASE_KEY = MESSAGES_DB_KEY + "database";
        private const string MESSAGES_DB_COLLECTION_KEY = MESSAGES_DB_KEY + "collection";
        private const string MESSAGES_STORAGE_TYPE = MESSAGES_DB_KEY + "telemetryStorageType";

        private const string ALARMS_DB_KEY = "TelemetryService:Alarms:";
        private const string ALARMS_DB_DATABASE_KEY = ALARMS_DB_KEY + "database";
        private const string ALARMS_DB_COLLECTION_KEY = ALARMS_DB_KEY + "collection";
        private const string ALARMS_DB_MAX_DELETE_RETRIES = ALARMS_DB_KEY + "maxDeleteRetries";

        private const string EXT_DEPENDENCIES_KEY = "ExternalDependencies:";
        private const string STORAGE_ADAPTER_API_URL_KEY = EXT_DEPENDENCIES_KEY + "storageAdapterWebServiceUrl";
        private const string STORAGE_ADAPTER_API_TIMEOUT_KEY = EXT_DEPENDENCIES_KEY + "storageAdapterWebserviceTimeout";
        private const string USER_MANAGEMENT_URL_KEY = EXT_DEPENDENCIES_KEY + "authWebServiceUrl";
        private const string DIAGNOSTICS_URL_KEY = EXT_DEPENDENCIES_KEY + "diagnosticsWebServiceUrl";
        private const string DIAGNOSTICS_MAX_LOG_RETRIES = EXT_DEPENDENCIES_KEY + "diagnosticsMaxLogRetries";

        private const string CLIENT_AUTH_KEY = APPLICATION_KEY + "ClientAuth:";
        private const string CORS_WHITELIST_KEY = CLIENT_AUTH_KEY + "corsWhitelist";
        private const string AUTH_TYPE_KEY = CLIENT_AUTH_KEY + "authType";
        private const string AUTH_REQUIRED_KEY = CLIENT_AUTH_KEY + "authRequired";

        private const string JWT_KEY = APPLICATION_KEY + "ClientAuth:JWT:";
        private const string JWT_ALGOS_KEY = JWT_KEY + "allowedAlgorithms";
        private const string JWT_ISSUER_KEY = JWT_KEY + "authIssuer";
        private const string JWT_AUDIENCE_KEY = JWT_KEY + "aadAppId";
        private const string JWT_CLOCK_SKEW_KEY = JWT_KEY + "clockSkewSeconds";

        private const string OPEN_ID_KEY = APPLICATION_KEY + "ClientAuth:OpenIdConnect:";
        private const string OPEN_ID_TTL_KEY = OPEN_ID_KEY + "timeToLiveDays";

        private const string ACTIONS_KEY = "Actions:";
        private const string ACTIONS_EVENTHUB_NAME = ACTIONS_KEY + "actionsEventHubName";
        private const string ACTIONS_EVENTHUB_CONNSTRING = ACTIONS_KEY + "actionsEventHubConnectionString";
        private const string ACTIONS_LOGICAPP_ENDPOINTURL = ACTIONS_KEY + "logicAppEndpointUrl";
        private const string ACTIONS_AZUREBLOB_CONNSTRING = ACTIONS_KEY + "storageConnectionString";
        private const string ACTIONS_AZUREBLOB_CONTAINER = ACTIONS_KEY + "storageContainer";
        private const string SOLUTION_URL = ACTIONS_KEY + "solutionWebsiteUrl";
        private const string TEMPLATE_FOLDER = ACTIONS_KEY + "templateFolder";

        public int Port { get; }
        public IServicesConfig ServicesConfig { get; }
        public IClientAuthConfig ClientAuthConfig { get; }

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
                StorageType = configData.GetString(MESSAGES_STORAGE_TYPE),
                CosmosDbConnString = configData.GetString(COSMOSDB_CONNSTRING_KEY),
                CosmosDbThroughput = configData.GetInt(COSMOSDB_RUS_KEY),
                StorageAdapterApiUrl = configData.GetString(STORAGE_ADAPTER_API_URL_KEY),
                StorageAdapterApiTimeout = configData.GetInt(STORAGE_ADAPTER_API_TIMEOUT_KEY),
                UserManagementApiUrl = configData.GetString(USER_MANAGEMENT_URL_KEY),
                TimeSeriesFqdn = configData.GetString(TIME_SERIES_FQDN),
                TimeSeriesAuthority = configData.GetString(TIME_SERIES_AUTHORITY),
                TimeSeriesAudience = configData.GetString(TIME_SERIES_AUDIENCE),
                TimeSeriesExplorerUrl = configData.GetString(TIME_SERIES_EXPLORER_URL),
                TimeSertiesApiVersion = configData.GetString(TIME_SERIES_API_VERSION),
                TimeSeriesTimeout = configData.GetString(TIME_SERIES_TIMEOUT),
                ActiveDirectoryTenant = configData.GetString(AAD_TENANT),
                ActiveDirectoryAppId = configData.GetString(AAD_APP_ID),
                ActiveDirectoryAppSecret = configData.GetString(AAD_APP_SECRET),
                DiagnosticsApiUrl = configData.GetString(DIAGNOSTICS_URL_KEY),
                DiagnosticsMaxLogRetries = configData.GetInt(DIAGNOSTICS_MAX_LOG_RETRIES),
                ActionsEventHubConnectionString = configData.GetString(ACTIONS_EVENTHUB_CONNSTRING),
                ActionsEventHubName = configData.GetString(ACTIONS_EVENTHUB_NAME),
                LogicAppEndpointUrl = configData.GetString(ACTIONS_LOGICAPP_ENDPOINTURL),
                BlobStorageConnectionString = configData.GetString(ACTIONS_AZUREBLOB_CONNSTRING),
                ActionsBlobStorageContainer = configData.GetString(ACTIONS_AZUREBLOB_CONTAINER),
                SolutionUrl = configData.GetString(SOLUTION_URL),
                TemplateFolder = AppContext.BaseDirectory + Path.DirectorySeparatorChar + configData.GetString(TEMPLATE_FOLDER)
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
                // By default the time to live for the OpenId connect token is 7 days
                OpenIdTimeToLive = TimeSpan.FromDays(configData.GetInt(OPEN_ID_TTL_KEY, 7))
            };
        }
    }
}
