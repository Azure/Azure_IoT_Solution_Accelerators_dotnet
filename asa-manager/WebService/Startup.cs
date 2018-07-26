// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.Auth;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService
{
    public class Startup
    {
        private readonly CancellationTokenSource agentsRunState;

        private SetupAgent.IAgent setupAgent;
        private DeviceGroupsAgent.IAgent deviceGroupsAgent;
        private TelemetryRulesAgent.IAgent telemetryRulesAgent;

        // Initialized in `Startup`
        public IConfigurationRoot Configuration { get; }

        // Initialized in `ConfigureServices`
        public IContainer ApplicationContainer { get; private set; }

        // Invoked by `Program.cs`
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddIniFile("appsettings.ini", optional: false, reloadOnChange: true);
            this.Configuration = builder.Build();
            this.agentsRunState = new CancellationTokenSource();
        }

        // This is where you register dependencies, add services to the
        // container. This method is called by the runtime, before the
        // Configure method below.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Setup (not enabling yet) CORS
            services.AddCors();

            // Add controllers as services so they'll be resolved.
            services.AddMvc().AddControllersAsServices();

            // Prepare DI container
            this.ApplicationContainer = DependencyResolution.Setup(services);

            // Print some useful information at bootstrap time
            this.PrintBootstrapInfo(this.ApplicationContainer);

            // Create the IServiceProvider based on the container
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method is called by the runtime, after the ConfigureServices
        // method above. Use this method to add middleware.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            ICorsSetup corsSetup,
            IApplicationLifetime appLifetime)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));

            // Check for Authorization header before dispatching requests
            app.UseMiddleware<AuthMiddleware>();

            // Enable CORS - Must be before UseMvc
            // see: https://docs.microsoft.com/en-us/aspnet/core/security/cors
            corsSetup.UseMiddleware(app);

            app.UseMvc();

            // Start agent threads
            appLifetime.ApplicationStarted.Register(this.StartAgents);
            appLifetime.ApplicationStopping.Register(this.StopAgents);

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(() => this.ApplicationContainer.Dispose());
        }

        private void StartAgents()
        {
            // Temporary workaround to allow twin JSON deserialization in IoT SDK
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                CheckAdditionalContent = false
            };

            this.setupAgent = this.ApplicationContainer.Resolve<SetupAgent.IAgent>();
            this.setupAgent.RunAsync(this.agentsRunState.Token);

            this.telemetryRulesAgent = this.ApplicationContainer.Resolve<TelemetryRulesAgent.IAgent>();
            this.telemetryRulesAgent.RunAsync(this.agentsRunState.Token);

            this.deviceGroupsAgent = this.ApplicationContainer.Resolve<DeviceGroupsAgent.IAgent>();
            this.deviceGroupsAgent.RunAsync(this.agentsRunState.Token);
        }

        private void StopAgents()
        {
            this.agentsRunState.Cancel();
        }

        private void PrintBootstrapInfo(IContainer container)
        {
            var log = container.Resolve<Services.Diagnostics.ILogger>();
            var config = container.Resolve<IConfig>();
            log.Warn("Service started", () => new { Uptime.ProcessId, LogLevel = config.LoggingConfig.LogLevel.ToString() });

            log.Info("Web service auth required: " + config.ClientAuthConfig.AuthRequired, () => { });
        }
    }
}
