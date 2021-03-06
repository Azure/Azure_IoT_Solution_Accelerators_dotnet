﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService.Auth
{
    public interface IClientAuthConfig
    {
        // CORS whitelist, in form { "origins": [], "methods": [], "headers": [] }
        // Defaults to empty, meaning No CORS.
        string CorsWhitelist { get; set; }

        // Whether CORS support is enabled
        // Default: false
        bool CorsEnabled { get; }

        // Whether the authentication and authorization is required or optional.
        // Default: true
        bool AuthRequired { get; set; }

        // Auth type: currently supports only "JWT"
        // Default: JWT
        string AuthType { get; set; }

        // The list of allowed signing algoritms
        // Default: RS256, RS384, RS512
        IEnumerable<string> JwtAllowedAlgos { get; set; }

        // The trusted issuer
        string JwtIssuer { get; set; }

        // The required audience
        string JwtAudience { get; set; }

        // Clock skew allowed when validating tokens expiration
        // Default: 2 minutes
        TimeSpan JwtClockSkew { get; set; }

        // Time to live for the OpenId Connect validation token.
        // The metadata settings will expire so the token needs to be
        // periodically recreated.
        // Default: 7 days
        TimeSpan OpenIdTimeToLive { get; set; }
    }

    public class ClientAuthConfig : IClientAuthConfig
    {
        public string CorsWhitelist { get; set; }
        public bool CorsEnabled => !string.IsNullOrEmpty(this.CorsWhitelist.Trim());

        public bool AuthRequired { get; set; }
        public string AuthType { get; set; }
        public IEnumerable<string> JwtAllowedAlgos { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
        public TimeSpan JwtClockSkew { get; set; }
        public TimeSpan OpenIdTimeToLive { get; set; }
    }
}
