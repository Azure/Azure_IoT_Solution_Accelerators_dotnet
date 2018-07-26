// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers.Helpers;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public class AlarmsByRuleController : Controller
    {
        private const int DEVICE_LIMIT = 200;

        private readonly IAlarms alarmService;
        private readonly IRules ruleService;
        private readonly ILogger log;

        public AlarmsByRuleController(
            IAlarms alarmService,
            IRules ruleService,
            ILogger logger)
        {
            this.alarmService = alarmService;
            this.ruleService = ruleService;
            this.log = logger;
        }

        [HttpGet]
        public async Task<AlarmByRuleListApiModel> ListAsync(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] string order,
            [FromQuery] int? skip,
            [FromQuery] int? limit,
            [FromQuery] string devices)
        {
            DateTimeOffset? fromDate = DateHelper.ParseDate(from);
            DateTimeOffset? toDate = DateHelper.ParseDate(to);

            if (order == null) order = "asc";
            if (skip == null) skip = 0;
            if (limit == null) limit = 1000;

            /* TODO: move this logic to the storage engine, depending on the
             * storage type the limit will be different. 200 is DocumentDb
             * limit for the IN clause.
             */
            string[] deviceIds = new string[0];
            if (!string.IsNullOrEmpty(devices))
            {
                deviceIds = devices.Split(',');
            }

            if (deviceIds.Length > DEVICE_LIMIT)
            {
                this.log.Warn("The client requested too many devices", () => new { devices.Length });
                throw new BadRequestException("The number of devices cannot exceed 200");
            }

            List<AlarmCountByRule> alarmsList 
                = await this.ruleService.GetAlarmCountForListAsync(
                fromDate,
                toDate,
                order,
                skip.Value,
                limit.Value,
                deviceIds);

            return new AlarmByRuleListApiModel(alarmsList);
        }

        [HttpGet("{id}")]
        public AlarmListByRuleApiModel Get(
            [FromRoute] string id,
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] string order,
            [FromQuery] int? skip,
            [FromQuery] int? limit,
            [FromQuery] string devices)
        {
            DateTimeOffset? fromDate = DateHelper.ParseDate(from);
            DateTimeOffset? toDate = DateHelper.ParseDate(to);

            if (order == null) order = "asc";
            if (skip == null) skip = 0;
            if (limit == null) limit = 1000;

            /* TODO: move this logic to the storage engine, depending on the
             * storage type the limit will be different. 200 is DocumentDb
             * limit for the IN clause.
             */
            string[] deviceIds = new string[0];
            if (!string.IsNullOrEmpty(devices))
            {
                deviceIds = devices.Split(',');
            }

            if (deviceIds.Length > DEVICE_LIMIT)
            {
                this.log.Warn("The client requested too many devices", () => new { devices.Length });
                throw new BadRequestException("The number of devices cannot exceed 200");
            }

            List<Alarm> alarmsList = this.alarmService.ListByRule(
                id,
                fromDate,
                toDate,
                order,
                skip.Value,
                limit.Value,
                deviceIds);

            return new AlarmListByRuleApiModel(alarmsList);
        }
    }
}
