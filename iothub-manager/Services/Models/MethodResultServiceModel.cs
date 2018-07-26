// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class MethodResultServiceModel
    {
        public int Status { get; set; }

        public string JsonPayload { get; set; }

        public MethodResultServiceModel()
        {
        }

        public MethodResultServiceModel(CloudToDeviceMethodResult result)
        {
            this.Status = result.Status;
            this.JsonPayload = result.GetPayloadAsJson();
        }
    }
}
