// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Concurrency;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;

namespace Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent
{
    public interface IAgent
    {
        Task RunAsync(CancellationToken runState);
    }

    public class Agent : IAgent
    {
        // Check if rules have been modified every 10 seconds
        // (not too frequently, but frequently enough to provide a good UX)
        private const int CHECK_INTERVAL_MSECS = 10000;

        private readonly ILogger log;
        private readonly IRules rulesService;
        private readonly IRulesWriter rulesWriter;
        private readonly IThreadWrapper thread;
        private IList<RuleApiModel> rules;
        private bool updateRequired;

        public Agent(
            IRules rulesService,
            IRulesWriter rulesWriter,
            IThreadWrapper thread,
            ILogger logger)
        {
            this.rulesService = rulesService;
            this.rulesWriter = rulesWriter;
            this.thread = thread;
            this.log = logger;
            this.updateRequired = false;
            this.rules = new List<RuleApiModel>();
        }

        /// <summary>
        /// Keep checking to see whether the list of rules have been modified.
        /// When the rules change, invoke the service responsible for exporting
        /// them to ASA.
        /// </summary>
        public async Task RunAsync(CancellationToken runState)
        {
            this.log.Info("ASA Job Configuration Agent running", () => { });

            while (!runState.IsCancellationRequested)
            {
                await this.CheckIfRulesHaveChangedAsync();
                await this.UpdateAsaIfNeededAsync();

                this.thread.Sleep(CHECK_INTERVAL_MSECS);
            }
        }

        private async Task UpdateAsaIfNeededAsync()
        {
            if (this.updateRequired)
            {
                try
                {
                    await this.rulesWriter.ExportRulesToAsaAsync(this.rules, DateTimeOffset.UtcNow);
                    this.updateRequired = false;
                }
                catch (Exception e)
                {
                    this.log.Error("Error while updating ASA", () => new { e });
                }
            }
        }

        private async Task CheckIfRulesHaveChangedAsync()
        {
            try
            {
                if (this.updateRequired || await this.RulesHaveChangedAsync())
                {
                    this.updateRequired = true;
                    this.log.Info("Device monitoring rules have changed", () => { });
                }
            }
            catch (Exception e)
            {
                this.log.Error("Error while checking the rules", () => new { e });
            }
        }

        private async Task<bool> RulesHaveChangedAsync()
        {
            var newRules = await this.rulesService.GetActiveRulesSortedByIdAsync();
            if (this.rulesService.RulesAreEquivalent(newRules, this.rules))
            {
                this.log.Debug("The old and new list of rules are equivalent, no change detected", () => { });
                return false;
            }

            this.log.Debug("The old and new list of rules are different", () => { });

            this.rules = newRules;
            return true;
        }
    }
}
