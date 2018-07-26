// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using AsaConfigAgent.Test.helpers;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace TelemetryRulesAgent.Test
{
    public class RulesWriterTest
    {
        // Protection against never ending tests, stop them and fail after 10 secs
        private const int TEST_TIMEOUT = 10;
        
        private readonly RulesWriter target;

        private readonly Mock<IBlobStorageHelper> blobStorageHelper;
        private readonly Mock<IBlobStorageConfig> blobStorageConfig;
        private readonly Mock<IFileWrapper> fileWrapper;
        private readonly Mock<ILogger> log;

        public RulesWriterTest(ITestOutputHelper log)
        {
            this.blobStorageHelper = new Mock<IBlobStorageHelper>();
            this.blobStorageConfig = new Mock<IBlobStorageConfig>();
            this.fileWrapper = new Mock<IFileWrapper>();
            this.log = new Mock<ILogger>();

            this.target = new RulesWriter(
                this.blobStorageConfig.Object,
                this.blobStorageHelper.Object,
                this.fileWrapper.Object,
                this.log.Object);

            this.blobStorageConfig.SetupGet(x => x.ReferenceDataDateFormat).Returns("yyyy-MM-dd");
            this.blobStorageConfig.SetupGet(x => x.ReferenceDataTimeFormat).Returns("HH-mm");
            this.blobStorageConfig.SetupGet(x => x.ReferenceDataRulesFileName).Returns("rules.json");
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItExportsRulesToTempFile()
        {
            // Arrange
            var rule1 = new RuleApiModel { Enabled = true, Name = "rule alpha" };
            var rule2 = new RuleApiModel { Enabled = true, Name = "rule beta" };
            var rules = new List<RuleApiModel> { rule1, rule2 };

            var filename = Guid.NewGuid().ToString();
            this.fileWrapper.Setup(x => x.GetTempFileName()).Returns(filename);

            // Act
            this.target.ExportRulesToAsaAsync(rules, DateTimeOffset.UtcNow)
                .Wait(TimeSpan.FromSeconds(TEST_TIMEOUT));

            // Assert
            this.fileWrapper.Verify(
                x => x.WriteAllText(filename, It.Is<string>(s => s.Contains(rule1.Name) && s.Contains(rule2.Name))), 
                Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItExportsRulesToBlob()
        {
            // Arrange
            var time = DateTimeOffset.UtcNow;
            var blobfile = Guid.NewGuid().ToString();
            
            var rule = new RuleApiModel { Enabled = true, Name = "rule alpha" };
            var rules = new List<RuleApiModel> { rule };

            var filename = Guid.NewGuid().ToString();
            this.fileWrapper.Setup(x => x.GetTempFileName()).Returns(filename);
            
            this.blobStorageConfig.SetupGet(x => x.ReferenceDataDateFormat).Returns("yyyy-MM-dd");
            this.blobStorageConfig.SetupGet(x => x.ReferenceDataTimeFormat).Returns("HH-mm");
            this.blobStorageConfig.SetupGet(x => x.ReferenceDataRulesFileName).Returns(blobfile);

            // Act
            this.target.ExportRulesToAsaAsync(rules, time)
                .Wait(TimeSpan.FromSeconds(TEST_TIMEOUT));

            // Assert
            var blobName = $"{time:yyyy-MM-dd/HH-mm}/{blobfile}";
            this.blobStorageHelper.Verify(
                x => x.WriteBlobFromFileAsync(blobName, filename), 
                Times.Once);
        }
    }
}
