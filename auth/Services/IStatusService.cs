// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.Auth.Services.Models;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.Auth.Services
{
    public interface IStatusService
    {
        StatusServiceModel GetStatusAsync();
    }
}
