// Copyright (c) Microsoft. All rights reserved.

using System;
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
    public sealed class MessagesController : Controller
    {
        private readonly IMessages messageService;
        private readonly ILogger log;

        public MessagesController(
            IMessages messageService,
            ILogger logger)
        {
            this.messageService = messageService;
            this.log = logger;
        }

        [HttpGet]
        public MessageListApiModel Get(
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

            // TODO: move this logic to the storage engine, depending on the
            // storage type the limit will be different. 200 is DocumentDb
            // limit for the IN clause.
            string[] deviceIds = new string[0];
            if (devices != null)
            {
                deviceIds = devices.Split(',');
            }
            if (deviceIds.Length > 200)
            {
                this.log.Warn("The client requested too many devices: {}", () => new { deviceIds.Length });
                throw new BadRequestException("The number of devices cannot exceed 200");
            }

            MessageList messageList = this.messageService.List(
                fromDate,
                toDate,
                order,
                skip.Value,
                limit.Value,
                deviceIds);

            return new MessageListApiModel(messageList);
        }
    }
}
