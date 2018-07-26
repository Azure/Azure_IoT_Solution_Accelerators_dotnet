// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models
{
    public class DeviceJobErrorApiModel
    {
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        public DeviceJobErrorApiModel()
        {
        }

        public DeviceJobErrorApiModel(DeviceJobErrorServiceModel error)
        {
            this.Code = error.Code;
            this.Description = error.Description;
        }
    }
}
