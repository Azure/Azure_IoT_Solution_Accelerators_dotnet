// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.Auth.Services;
using Microsoft.Azure.IoTSolutions.Auth.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.Auth.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IConfig config;
        private readonly IStatusService statusService;

        public StatusController(IConfig config, IStatusService statusService)
        {
            this.config = config;
            this.statusService = statusService;
        }

        public StatusApiModel GetAsync()
        {
            var result = new StatusApiModel(this.statusService.GetStatusAsync());
            result.Properties.Add("AuthRequired", this.config.ClientAuthConfig?.AuthRequired.ToString());
            result.Properties.Add("Port", this.config.Port.ToString());
            return result;
        }
    }
}
