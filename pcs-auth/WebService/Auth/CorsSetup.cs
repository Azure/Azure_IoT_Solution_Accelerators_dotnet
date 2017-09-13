﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.WebService.Runtime;
using Newtonsoft.Json;
using System.Linq;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.Auth
{
    public interface ICorsSetup
    {
        void useMiddleware(IApplicationBuilder app);
    }

    public class CorsSetup : ICorsSetup
    {
        private readonly IClientAuthConfig config;
        private readonly ILogger log;
        private readonly bool enabled;

        public CorsSetup(
            IClientAuthConfig config,
            ILogger logger)
        {
            this.config = config;
            this.log = logger;
            this.enabled = !string.IsNullOrEmpty(this.config.CorsWhitelist.Trim());
        }

        public void useMiddleware(IApplicationBuilder app)
        {
            if (this.enabled)
            {
                app.UseCors(this.BuildCorsPolicy);
            }
        }

        private void BuildCorsPolicy(CorsPolicyBuilder builder)
        {
            CorsWhitelistModel model;
            try
            {
                model = JsonConvert.DeserializeObject<CorsWhitelistModel>(this.config.CorsWhitelist);
                if (model == null)
                {
                    this.log.Error("Invalid CORS whitelist. Ignored", () => new { this.config.CorsWhitelist });
                    return;
                }
            }
            catch (Exception ex)
            {
                this.log.Error("Invalid CORS whitelist. Ignored", () => new { this.config.CorsWhitelist, ex.Message });
                return;
            }

            if (model.Origins == null)
            {
                this.log.Info("No setting for CORS origin policy was found, ignore", () => { });
            }
            else if (model.Origins.Contains("*"))
            {
                this.log.Info("CORS policy allowed any origin", () => { });
                builder.AllowAnyOrigin();
            }
            else
            {
                this.log.Info("Add specified origins to CORS policy", () => new { model.Origins });
                builder.WithOrigins(model.Origins);
            }

            if (model.Origins == null)
            {
                this.log.Info("No setting for CORS method policy was found, ignore", () => { });
            }
            else if (model.Methods.Contains("*"))
            {
                this.log.Info("CORS policy allowed any method", () => { });
                builder.AllowAnyMethod();
            }
            else
            {
                this.log.Info("Add specified methods to CORS policy", () => new { model.Methods });
                builder.WithMethods(model.Methods);
            }

            if (model.Origins == null)
            {
                this.log.Info("No setting for CORS header policy was found, ignore", () => { });
            }
            else if (model.Headers.Contains("*"))
            {
                this.log.Info("CORS policy allowed any header", () => { });
                builder.AllowAnyHeader();
            }
            else
            {
                this.log.Info("Add specified headers to CORS policy", () => new { model.Headers });
                builder.WithHeaders(model.Headers);
            }
        }
    }
}
