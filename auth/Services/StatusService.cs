// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.Auth.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using Microsoft.Azure.IoTSolutions.Auth.Services.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    class StatusService : IStatusService
    {
        private readonly ILogger log;
        private readonly IServicesConfig servicesConfig;

        public StatusService(
            ILogger logger,
            IServicesConfig servicesConfig
        )
        {
            this.log = logger;
            this.servicesConfig = servicesConfig;
        }

        public async Task<StatusServiceModel> GetStatusAsync()
        {
            var result = new StatusServiceModel(true, "Alive and well!");
            var errors = new List<string>();

            // TODO: Check connection to AAD

            this.log.Info(
                "Service status request",
                () => new
                {
                    Healthy = result.Status.IsHealthy,
                    result.Status.Message
                });

            if (errors.Count > 0)
            {
                result.Status.Message = string.Join("; ", errors);
            }

            return result;
        }

        private void SetServiceStatus(
            string dependencyName,
            StatusResultServiceModel serviceResult,
            StatusServiceModel result,
            List<string> errors
        )
        {
            if (!serviceResult.IsHealthy)
            {
                errors.Add(dependencyName + " check failed");
                result.Status.IsHealthy = false;
            }

            result.Dependencies.Add(dependencyName, serviceResult);
        }
    }
}