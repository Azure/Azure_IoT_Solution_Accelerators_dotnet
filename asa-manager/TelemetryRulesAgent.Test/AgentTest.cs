// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using AsaConfigAgent.Test.helpers;
using Microsoft.Azure.IoTSolutions.AsaManager.Services;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Concurrency;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace TelemetryRulesAgent.Test
{
    public class AgentTest
    {
        // Protection against never ending tests, stop them and fail after 10 secs
        private const int TEST_TIMEOUT = 10;

        private readonly Agent target;
        private readonly Mock<IRules> rulesService;
        private readonly Mock<IRulesWriter> asaRulesConfigService;
        private readonly Mock<IThreadWrapper> thread;
        private readonly Mock<ILogger> logger;

        private CancellationTokenSource agentsRunState;
        private CancellationToken runState;

        public AgentTest(ITestOutputHelper log)
        {
            this.rulesService = new Mock<IRules>();
            this.asaRulesConfigService = new Mock<IRulesWriter>();
            this.thread = new Mock<IThreadWrapper>();
            this.logger = new Mock<ILogger>();
            this.agentsRunState = new CancellationTokenSource();
            this.runState = this.agentsRunState.Token;

            this.target = new Agent(
                this.rulesService.Object,
                this.asaRulesConfigService.Object,
                this.thread.Object,
                this.logger.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItKeepsCheckingIfRulesChanged()
        {
            // Arrange
            var loops = 8;
            this.ThereAreNoRules();
            this.StopAgentAfterNLoops(loops);

            // Act
            this.target.RunAsync(this.runState).Wait(TimeSpan.FromSeconds(TEST_TIMEOUT));

            // Assert
            this.thread.Verify(
                x => x.Sleep(It.IsAny<int>()),
                Times.Exactly(loops));
            this.rulesService.Verify(
                x => x.GetActiveRulesSortedByIdAsync(),
                Times.Exactly(loops));
            this.asaRulesConfigService.Verify(
                x => x.ExportRulesToAsaAsync(It.IsAny<IList<RuleApiModel>>(), It.IsAny<DateTimeOffset>()),
                Times.Never);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItTriggersAConfigUpdateWhenRulesChange()
        {
            // Arrange
            this.RulesHaveNotChanged();
            this.StopAgentAfterNLoops(2);

            // Act
            this.target.RunAsync(this.runState).Wait(TimeSpan.FromSeconds(TEST_TIMEOUT));

            // Assert
            this.asaRulesConfigService.Verify(
                x => x.ExportRulesToAsaAsync(It.IsAny<IList<RuleApiModel>>(), It.IsAny<DateTimeOffset>()),
                Times.Never);

            // Arrange
            this.RulesHaveChanged();
            this.StopAgentAfterNLoops(1);

            // Act
            this.target.RunAsync(this.runState).Wait(TimeSpan.FromSeconds(TEST_TIMEOUT));

            // Assert
            this.asaRulesConfigService.Verify(
                x => x.ExportRulesToAsaAsync(It.IsAny<IList<RuleApiModel>>(), It.IsAny<DateTimeOffset>()),
                Times.Once);
        }

        private void RulesHaveNotChanged()
        {
            this.rulesService.Setup(
                    x => x.RulesAreEquivalent(It.IsAny<IList<RuleApiModel>>(), It.IsAny<IList<RuleApiModel>>()))
                .Returns(true);
        }

        private void RulesHaveChanged()
        {
            this.rulesService.Setup(
                    x => x.RulesAreEquivalent(It.IsAny<IList<RuleApiModel>>(), It.IsAny<IList<RuleApiModel>>()))
                .Returns(false);
        }

        private void ThereAreNoRules()
        {
            this.rulesService.Setup(
                    x => x.GetActiveRulesSortedByIdAsync())
                .ReturnsAsync(new List<RuleApiModel>());

            // Make sure that 2 empty lists are considered equivalent
            this.rulesService.Setup(
                    x => x.RulesAreEquivalent(
                        It.Is<IList<RuleApiModel>>(l => l.Count == 0),
                        It.Is<IList<RuleApiModel>>(l => l.Count == 0)))
                .Returns(true);
        }

        private void ThereAreNRules(int i)
        {
            var list = new List<RuleApiModel>();
            for (int j = 0; j < i; j++)
            {
                list.Add(new RuleApiModel());
            }

            this.rulesService.Setup(
                    x => x.GetActiveRulesSortedByIdAsync())
                .ReturnsAsync(list);
        }

        private void StopAgentAfterNLoops(int n)
        {
            // A new cancellation token is required every time
            this.agentsRunState = new CancellationTokenSource();
            this.runState = this.agentsRunState.Token;

            this.thread
                .Setup(x => x.Sleep(It.IsAny<int>()))
                .Callback(() =>
                {
                    if (--n <= 0) this.agentsRunState.Cancel();
                });
        }
    }
}
