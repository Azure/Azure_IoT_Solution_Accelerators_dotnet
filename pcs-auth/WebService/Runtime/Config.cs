// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;
using Microsoft.Azure.IoTSolutions.Auth.WebService.Auth;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.Runtime
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
        private const string APPLICATION_KEY = "AuthService:";
        private const string PORT_KEY = APPLICATION_KEY + "webservice_port";
        private const string JWT_USER_ID_FROM_KEY = APPLICATION_KEY + "extract_userid_from";
        private const string JWT_NAME_FROM_KEY = APPLICATION_KEY + "extract_name_from";
        private const string JWT_EMAIL_FROM_KEY = APPLICATION_KEY + "extract_email_from";
        private const string JWT_ROLES_FROM_KEY = APPLICATION_KEY + "extract_roles_from";
        private const string POLICIES_FOLDER_KEY = APPLICATION_KEY + "policies_folder";

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

            this.ServicesConfig = new ServicesConfig
            {
                JwtUserIdFrom = configData.GetString(JWT_USER_ID_FROM_KEY, "oid").Split(','),
                JwtNameFrom = configData.GetString(JWT_NAME_FROM_KEY, "given_name,family_name").Split(','),
                JwtEmailFrom = configData.GetString(JWT_EMAIL_FROM_KEY, "email").Split(','),
                JwtRolesFrom = configData.GetString(JWT_ROLES_FROM_KEY, "roles"),
                PoliciesFolder = MapRelativePath(configData.GetString(POLICIES_FOLDER_KEY))
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
        }

        private static string MapRelativePath(string path)
        {
            return path.StartsWith(".") ? Path.Combine(AppContext.BaseDirectory, path) : path;
        }
    }
}
