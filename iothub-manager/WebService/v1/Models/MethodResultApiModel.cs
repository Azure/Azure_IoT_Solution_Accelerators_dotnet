// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class MethodResultApiModel : MethodResultServiceModel
    {
        public MethodResultApiModel(MethodResultServiceModel model)
        {
            this.Status = model.Status;
            this.JsonPayload = model.JsonPayload;
        }
    }
}
