// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.TelemetryRulesAgent
{
    public interface IRulesWriter
    {
        // Export the rules for ASA to execute
        Task ExportRulesToAsaAsync(IList<RuleApiModel> rules, DateTimeOffset time);
    }

    public class RulesWriter : IRulesWriter
    {
        private readonly IBlobStorageHelper blobStorageHelper;
        private readonly IBlobStorageConfig blobStorageConfig;
        private readonly IFileWrapper fileWrapper;
        private readonly ILogger log;

        public RulesWriter(
            IBlobStorageConfig blobStorageConfig,
            IBlobStorageHelper blobStorageHelper,
            IFileWrapper fileWrapper,
            ILogger logger)
        {
            this.blobStorageConfig = blobStorageConfig;
            this.log = logger;
            this.blobStorageHelper = blobStorageHelper;
            this.fileWrapper = fileWrapper;
        }

        public async Task ExportRulesToAsaAsync(IList<RuleApiModel> rules, DateTimeOffset time)
        {
            string fileName = this.fileWrapper.GetTempFileName();
            string fileContent = this.RulesToJson(rules);

            this.log.Debug("Exporting rules to temporary file", () => new { fileName, contentSize = fileContent.Length });

            try
            {
                this.fileWrapper.WriteAllText(fileName, fileContent);
                await this.blobStorageHelper.WriteBlobFromFileAsync(
                    this.GetBlobName(time),
                    fileName);
            }
            catch (Exception e)
            {
                this.log.Error("Unable to create rules reference data", () => new { e });
                throw new ExternalDependencyException("Unable to create rules reference data", e);
            }
        }

        // Note: here all the rules should be active; inactive rules are filtered out earlier.
        private string RulesToJson(IEnumerable<RuleApiModel> rules)
        {
            var list = rules.Select(rule => new AsaRefDataRule(rule)).ToList();
            return JsonConvert.SerializeObject(list, Formatting.Indented);
        }

        // Provide the filename, taking care of ASA path pattern
        private string GetBlobName(DateTimeOffset time)
        {
            var dateTimeFormatString = $"{this.blobStorageConfig.ReferenceDataDateFormat}/{this.blobStorageConfig.ReferenceDataTimeFormat}";
            var formattedDate = time.ToString(dateTimeFormatString);
            var name = $"{formattedDate}/{this.blobStorageConfig.ReferenceDataRulesFileName}";

            this.log.Debug("Blob name", () => new { name });

            return name;
        }
    }
}
