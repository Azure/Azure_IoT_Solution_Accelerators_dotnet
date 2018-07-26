// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public interface IPolicies
    {
        IEnumerable<Policy> GetList();
        Policy GetByRole(string role);
    }

    public class Policies : IPolicies
    {
        private const string EXT = ".json";

        private readonly IServicesConfig config;
        private readonly ILogger log;

        private List<string> policyFiles;
        private List<Policy> policies;

        public Policies(
            IServicesConfig config,
            ILogger logger)
        {
            this.config = config;
            this.log = logger;
            this.policyFiles = null;
            this.policies = null;
        }

        public IEnumerable<Policy> GetList()
        {
            if (this.policies != null)
            {
                return this.policies;
            }

            // Retrieve policies if policy list is null
            this.policies = new List<Policy>();
            try
            {
                var files = this.GetPolicyFiles();
                foreach (var f in files)
                {
                    // Deserialize the list of policies and add to list
                    var policyList = JsonConvert.DeserializeObject<PolicyList>(File.ReadAllText(f));
                    this.policies.AddRange(policyList.Items);
                }
            }
            catch (Exception e)
            {
                this.log.Error("Unable to load policy file configuration.", () => new { e.Message, Exception = e });

                throw new InvalidConfigurationException("Unable to load policy file configuration: " + e.Message, e);
            }

            return this.policies;
        }

        public Policy GetByRole(string role)
        {
            var list = this.GetList();
            var item = list.FirstOrDefault(i => i.Role.Equals(role, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                return item;
            }

            throw new ResourceNotFoundException("No policy configured for a role by the name of '" + role + "'.");
        }

        private List<string> GetPolicyFiles()
        {
            if (this.policyFiles != null)
            {
                return this.policyFiles;
            }

            this.log.Debug("Policies folder", () => new { this.config.PoliciesFolder });
            var fileEntries = Directory.GetFiles(this.config.PoliciesFolder);
            this.policyFiles = fileEntries.Where(fileName => fileName.EndsWith(EXT)).ToList();
            this.log.Debug("Policy files", () => new { this.policyFiles });

            return this.policyFiles;
        }
    }
}
