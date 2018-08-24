// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.StorageAdapter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services
{
    public interface IRules
    {
        Task CreateFromTemplateAsync(string template);

        Task DeleteAsync(string id);

        Task<Rule> GetAsync(string id);

        Task<List<Rule>> GetListAsync(
            string order,
            int skip,
            int limit,
            string groupId,
            bool includeDeleted);

        Task<List<AlarmCountByRule>> GetAlarmCountForListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices);

        Task<Rule> CreateAsync(Rule rule);

        Task<Rule> UpsertIfNotDeletedAsync(Rule rule);
    }

    public class Rules : IRules
    {
        private const string STORAGE_COLLECTION = "rules";
        private const string INVALID_CHARACTER = @"[^A-Za-z0-9:;.,_\-]";
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";

        private readonly IStorageAdapterClient storage;
        private readonly ILogger log;

        private readonly IAlarms alarms;

        public Rules(
            IStorageAdapterClient storage,
            ILogger logger,
            IAlarms alarms)
        {
            this.storage = storage;
            this.log = logger;
            this.alarms = alarms;
        }

        public async Task CreateFromTemplateAsync(string template)
        {
            string pathToTemplate = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                @"Data\Rules\" + template + ".json");

            if (RuleTemplateValidator.IsValid(pathToTemplate))
            {
                var file = JToken.Parse(File.ReadAllText(pathToTemplate));

                foreach (var item in file["Rules"])
                {
                    Rule newRule = this.Deserialize(item.ToString());

                    await this.CreateAsync(newRule);
                }
            }
        }

        public async Task DeleteAsync(string id)
        {
            Rule existing;
            try
            {
                existing = await this.GetAsync(id);
            }
            catch (ResourceNotFoundException exception)
            {
                this.log.Debug("Tried to delete rule which did not exist", () => new { id, exception });
                return;
            }
            catch (Exception exception)
            {
                this.log.Error("Error trying to delete rule", () => new { id, exception });
                throw exception;
            }

            if (existing.Deleted)
            {
                return;
            }

            existing.Deleted = true;

            var item = JsonConvert.SerializeObject(existing);
            await this.storage.UpsertAsync(
                STORAGE_COLLECTION,
                existing.Id,
                item,
                existing.ETag);
        }

        public async Task<Rule> GetAsync(string id)
        {
            if (Regex.IsMatch(id, INVALID_CHARACTER))
            {
                this.log.Debug("id contains illegal characters.", () => new { id });
                throw new InvalidInputException("id contains illegal characters.");
            }

            var item = await this.storage.GetAsync(STORAGE_COLLECTION, id);
            var rule = this.Deserialize(item.Data);

            rule.ETag = item.ETag;
            rule.Id = item.Key;

            return rule;
        }

        public async Task<List<Rule>> GetListAsync(
            string order,
            int skip,
            int limit,
            string groupId,
            bool includeDeleted)
        {
            var data = await this.storage.GetAllAsync(STORAGE_COLLECTION);
            var ruleList = new List<Rule>();
            foreach (var item in data.Items)
            {
                try
                {
                    var rule = this.Deserialize(item.Data);
                    rule.ETag = item.ETag;
                    rule.Id = item.Key;

                    if ((string.IsNullOrEmpty(groupId) ||
                        rule.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
                        && (!rule.Deleted || includeDeleted))
                    {
                        ruleList.Add(rule);
                    }
                }
                catch (Exception e)
                {
                    this.log.Debug("Could not parse result from Key Value Storage",
                        () => new { e });
                    throw new InvalidDataException(
                        "Could not parse result from Key Value Storage", e);
                }
            }

            // sort based on MessageTime, default descending
            ruleList.Sort();

            if (order.Equals("asc", StringComparison.OrdinalIgnoreCase))
            {
                ruleList.Reverse();
            }

            if (skip >= ruleList.Count)
            {
                this.log.Debug("Skip value greater than size of list returned",
                    () => new { skip, ruleList.Count });

                return new List<Rule>();
            }
            else if ((limit + skip) >= ruleList.Count)
            {
                // if requested values are out of range, return remaining items
                return ruleList.GetRange(skip, ruleList.Count - skip);
            }

            return ruleList.GetRange(skip, limit);
        }

        public async Task<List<AlarmCountByRule>> GetAlarmCountForListAsync(
            DateTimeOffset? from,
            DateTimeOffset? to,
            string order,
            int skip,
            int limit,
            string[] devices)
        {
            var alarmCountByRuleList = new List<AlarmCountByRule>();

            // get list of rules
            var rulesList = await this.GetListAsync(order, skip, limit, null, true);

            // get open alarm count and most recent alarm for each rule
            foreach (var rule in rulesList)
            {
                var alarmCount = this.alarms.GetCountByRule(
                    rule.Id,
                    from,
                    to,
                    devices);

                // skip to next rule if no alarms found
                if (alarmCount == 0)
                {
                    continue;
                }

                // get most recent alarm for rule
                var recentAlarm = this.GetLastAlarmForRule(rule.Id, from, to, devices);

                // add alarmCountByRule to list
                alarmCountByRuleList.Add(
                    new AlarmCountByRule(
                        alarmCount,
                        recentAlarm.Status,
                        recentAlarm.DateCreated,
                        rule));
            }

            return alarmCountByRuleList;
        }

        public async Task<Rule> CreateAsync(Rule rule)
        {
            if (rule == null)
            {
                throw new InvalidInputException("Rule not provided.");
            }

            // Ensure dates are correct
            rule.DateCreated = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
            rule.DateModified = rule.DateCreated;

            var item = JsonConvert.SerializeObject(rule);
            var result = await this.storage.CreateAsync(STORAGE_COLLECTION, item);

            Rule newRule = this.Deserialize(result.Data);
            newRule.ETag = result.ETag;

            if (string.IsNullOrEmpty(newRule.Id)) newRule.Id = result.Key;

            return newRule;
        }

        public async Task<Rule> UpsertIfNotDeletedAsync(Rule rule)
        {
            if (rule == null)
            {
                throw new InvalidInputException("Rule not provided.");
            }

            // Ensure dates are correct
            // Get the existing rule so we keep the created date correct
            Rule savedRule = null;
            try
            {
                savedRule = await this.GetAsync(rule.Id);
            }
            catch (ResourceNotFoundException e)
            {
                // Following the pattern of Post should create or update
                this.log.Info("Rule not found will create new rule for Id:", () => new { rule.Id, e });
            }

            if (savedRule != null && savedRule.Deleted)
            {
                throw new ResourceNotFoundException($"Rule {rule.Id} not found");
            }

            return await this.UpsertAsync(rule, savedRule);
        }

        private async Task<Rule> UpsertAsync(Rule rule, Rule savedRule)
        {
            // If rule does not exist and id is provided upsert rule with that id
            if (savedRule == null && rule.Id != null)
            {
                rule.DateCreated = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
                rule.DateModified = rule.DateCreated;
            }
            else // update rule with stored date created
            {
                rule.DateCreated = savedRule.DateCreated;
                rule.DateModified = DateTimeOffset.UtcNow.ToString(DATE_FORMAT);
            }

            // Save the updated rule if it exists or create new rule with id
            var item = JsonConvert.SerializeObject(rule);
            var result = await this.storage.UpsertAsync(
                STORAGE_COLLECTION,
                rule.Id,
                item,
                rule.ETag);

            Rule updatedRule = this.Deserialize(result.Data);

            updatedRule.ETag = result.ETag;
            updatedRule.Id = result.Key;

            return updatedRule;
        }

        private Alarm GetLastAlarmForRule(
            string id,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string[] devices)
        {
            var resultList = this.alarms.ListByRule(
                id,
                from,
                to,
                "desc",
                0,
                1,
                devices);

            if (resultList.Count != 0)
            {
                return resultList[0];
            }
            else
            {
                this.log.Debug("Could not retrieve most recent alarm", () => new { id });
                throw new ExternalDependencyException(
                    "Could not retrieve most recent alarm");
            }
        }

        private Rule Deserialize(string jsonRule)
        {
            try
            {
                return JsonConvert.DeserializeObject<Rule>(jsonRule);
            }
            catch (Exception e)
            {
                throw new ExternalDependencyException("Unable to parse data.", e);
            }
        }
    }
}
