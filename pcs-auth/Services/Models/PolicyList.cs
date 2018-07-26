// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.Auth.Services.Models
{
    public class PolicyList
    {
        public List<Policy> Items { get; set; }

        public PolicyList() => this.Items = new List<Policy>();
    }
}
