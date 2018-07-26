// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeviceJobErrorServiceModel
    {
        public string Code { get; }
        public string Description { get; }

        public DeviceJobErrorServiceModel(DeviceJobError error)
        {
            this.Code = error.Code;
            this.Description = error.Description;
        }
    }
}
