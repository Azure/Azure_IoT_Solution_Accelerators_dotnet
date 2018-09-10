// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services
{
    public interface ISeed
    {
        Task TrySeedAsync();
    }

    public class Seed : ISeed
    {
        private const string SEED_COLLECTION_ID = "solution-settings";
        private const string MUTEX_KEY = "seedMutex";
        private const string COMPLETED_FLAG_KEY = "seedCompleted";
        private readonly TimeSpan mutexTimeout = TimeSpan.FromMinutes(5);

        private readonly IServicesConfig config;
        private readonly IStorageMutex mutex;
        private readonly IStorage storage;
        private readonly IStorageAdapterClient storageClient;
        private readonly IDeviceSimulationClient simulationClient;
        private readonly IDeviceTelemetryClient telemetryClient;
        private readonly ILogger log;

        public Seed(
            IServicesConfig config,
            IStorageMutex mutex,
            IStorage storage,
            IStorageAdapterClient storageClient,
            IDeviceSimulationClient simulationClient,
            IDeviceTelemetryClient telemetryClient,
            ILogger logger)
        {
            this.config = config;
            this.mutex = mutex;
            this.storage = storage;
            this.storageClient = storageClient;
            this.simulationClient = simulationClient;
            this.telemetryClient = telemetryClient;
            this.log = logger;
        }

        public async Task TrySeedAsync()
        {
            if (string.Equals(this.config.SolutionType, "devicesimulation-nohub", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!await this.mutex.EnterAsync(SEED_COLLECTION_ID, MUTEX_KEY, this.mutexTimeout))
            {
                this.log.Info("Seed skipped (conflict)", () => { });
                return;
            }

            if (await this.CheckCompletedFlagAsync())
            {
                this.log.Info("Seed skipped (completed)", () => { });
                return;
            }

            this.log.Info("Seed begin", () => { });
            await this.SeedAsync();
            this.log.Info("Seed end", () => { });

            await this.SetCompletedFlagAsync();

            await this.mutex.LeaveAsync(SEED_COLLECTION_ID, MUTEX_KEY);
        }

        private async Task<bool> CheckCompletedFlagAsync()
        {
            try
            {
                await this.storageClient.GetAsync(SEED_COLLECTION_ID, COMPLETED_FLAG_KEY);
                return true;
            }
            catch (ResourceNotFoundException)
            {
                return false;
            }
        }

        private async Task SetCompletedFlagAsync()
        {
            await this.storageClient.UpdateAsync(SEED_COLLECTION_ID, COMPLETED_FLAG_KEY, "true", "*");
        }

        private async Task SeedAsync()
        {
            if (string.Equals(this.config.SolutionType, "devicesimulation", StringComparison.OrdinalIgnoreCase))
            {
                await this.SeedDeviceSimulationAsync();
            }
            else
            {
                await this.SeedSingleTemplateAsync();
            }
        }

        // Seed single template for Remote Monitoring solution
        private async Task SeedSingleTemplateAsync()
        {
            var templateName = this.config.SeedTemplate;
            var template = this.GetSeedContent(templateName);

            if (template.Groups.Select(g => g.Id).Distinct().Count() != template.Groups.Count())
            {
                this.log.Warn("Found duplicated group ID", () => new { template.Groups });
            }

            if (template.Rules.Select(r => r.Id).Distinct().Count() != template.Rules.Count())
            {
                this.log.Warn("Found duplicated rule ID", () => new { template.Rules });
            }

            var groupIds = new HashSet<string>(template.Groups.Select(g => g.Id));
            var rulesWithInvalidGroupId = template.Rules.Where(r => !groupIds.Contains(r.GroupId));
            if (rulesWithInvalidGroupId.Any())
            {
                this.log.Warn("Invalid group ID found in rules", () => new { rulesWithInvalidGroupId });
            }

            foreach (var group in template.Groups)
            {
                try
                {
                    await this.storage.UpdateDeviceGroupAsync(group.Id, group, "*");
                }
                catch (Exception ex)
                {
                    this.log.Error($"Failed to seed default group {group.DisplayName}", () => new { group, ex.Message });
                    throw;
                }
            }

            foreach (var rule in template.Rules)
            {
                try
                {
                    await this.telemetryClient.UpdateRuleAsync(rule, "*");
                }
                catch (Exception ex)
                {
                    this.log.Error($"Failed to seed default rule {rule.Description}", () => new { rule, ex.Message });
                    throw;
                }
            }

            try
            {
                var simulationModel = await this.simulationClient.GetDefaultSimulationAsync();

                if (simulationModel != null)
                {
                    this.log.Info("Skip seed simulation since there is already one simulation", () => new { simulationModel });
                }
                else
                {
                    foreach (var simulation in template.Simulations)
                    {
                        await this.simulationClient.UpdateSimulationAsync(simulation);
                    }
                }
            }
            catch (Exception ex)
            {
                this.log.Error("Failed to seed default simulation", () => new { ex.Message });
                throw;
            }
        }

        // Seed single template for Device Simulation solution
        private async Task SeedDeviceSimulationAsync()
        {
            try
            {
                var simulationModel = await this.simulationClient.GetDefaultSimulationAsync();

                if (simulationModel != null)
                {
                    this.log.Info("Skip seed simulation since there is already one simulation", () => new { simulationModel });
                }
                else
                {
                    var template = this.GetSeedContent("device-simulation-template");

                    foreach (var simulation in template.Simulations)
                    {
                        await this.simulationClient.UpdateSimulationAsync(simulation);
                    }
                }
            }
            catch (Exception ex)
            {
                this.log.Error("Failed to seed default simulations", () => new { ex.Message });
                throw;
            }
        }

        private Template GetSeedContent(string templateName)
        {
            string content;
            var root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var file = Path.Combine(root, "Data", $"{templateName}.json");
            if (!File.Exists(file))
            {
                // ToDo: Check if `template` is a valid URL and try to load the content

                throw new ResourceNotFoundException($"Template {templateName} does not exist");
            }
            else
            {
                content = File.ReadAllText(file);
            }

            Template template;

            try
            {
                template = JsonConvert.DeserializeObject<Template>(content);
            }
            catch (Exception ex)
            {
                throw new InvalidInputException("Failed to parse template", ex);
            }

            return template;
        }
    }
}
