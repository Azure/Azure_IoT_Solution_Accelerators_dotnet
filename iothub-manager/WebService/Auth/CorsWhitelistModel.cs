// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Auth
{
    class CorsWhitelistModel
    {
        public string[] Origins { get; set; }
        public string[] Methods { get; set; }
        public string[] Headers { get; set; }
    }
}
