// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class RulesController : Controller
    {
        private readonly IRules ruleService;

        public RulesController(IRules ruleService)
        {
            this.ruleService = ruleService;
        }

        [HttpGet("{id}")]
        public async Task<RuleApiModel> GetAsync([FromRoute] string id)
        {
            Rule rule = await this.ruleService.GetAsync(id);
            return new RuleApiModel(rule, rule.Deleted);
        }

        [HttpGet]
        public async Task<RuleListApiModel> ListAsync(
            [FromQuery] string order,
            [FromQuery] int? skip,
            [FromQuery] int? limit,
            [FromQuery] string groupId,
            [FromQuery] bool? includeDeleted)
        {
            if (order == null) order = "asc";
            if (skip == null) skip = 0;
            if (limit == null) limit = 1000;
            if (includeDeleted == null) includeDeleted = false;

            return new RuleListApiModel(
                await this.ruleService.GetListAsync(
                    order,
                    skip.Value,
                    limit.Value,
                    groupId,
                    includeDeleted.Value),
                includeDeleted.Value);
        }

        [HttpPost]
        public async Task<RuleApiModel> PostAsync(
            [FromQuery] string template,
            [FromBody] RuleApiModel rule)
        {
            if (!string.IsNullOrEmpty(template))
            {
                // create rules from template
                await this.ruleService.CreateFromTemplateAsync(template);
                return null;
            }

            // create rule from request body
            if (rule == null)
            {
                throw new InvalidInputException("Rule not provided in request body.");
            }
            Rule newRule = await this.ruleService.CreateAsync(rule.ToServiceModel());

            return new RuleApiModel(newRule, false);
        }

        [HttpPut("{id}")]
        public async Task<RuleApiModel> PutAsync(
            [FromRoute] string id,
            [FromBody] RuleApiModel rule)
        {
            if (rule == null)
            {
                throw new InvalidInputException("Rule not provided in request body.");
            }

            //Ensure the id on the model matches the route
            rule.Id = id;
            Rule updatedRule = await this.ruleService.UpsertIfNotDeletedAsync(rule.ToServiceModel());

            return new RuleApiModel(updatedRule, false);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync([FromRoute] string id)
        {
            await this.ruleService.DeleteAsync(id);
        }
    }
}
