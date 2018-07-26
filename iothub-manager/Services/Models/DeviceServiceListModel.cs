// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeviceServiceListModel
    {
        public string ContinuationToken { get; set; }

        public List<DeviceServiceModel> Items { get; set; }

        public DeviceServiceListModel(IEnumerable<DeviceServiceModel> devices, string continuationToken = null)
        {
            this.ContinuationToken = continuationToken;
            this.Items = new List<DeviceServiceModel>(devices);
        }

        public DeviceTwinName GetDeviceTwinNames()
        {
            if (this.Items?.Count > 0)
            {
                var tagSet = new HashSet<string>();
                var reportedSet = new HashSet<string>();
                this.Items.ForEach(m =>
                {
                    foreach (var item in m.Twin.Tags)
                    {
                        this.PrepareTagNames(tagSet, item.Value, item.Key);
                    }
                    foreach (var item in m.Twin.ReportedProperties)
                    {
                        this.PrepareTagNames(reportedSet, item.Value, item.Key);
                    }
                });
                return new DeviceTwinName { Tags = tagSet, ReportedProperties = reportedSet };
            }
            return null;
        }

        private void PrepareTagNames(HashSet<string> set, JToken jToken, string prefix)
        {
            if (jToken is JValue)
            {
                set.Add(prefix);
                return;
            }
            foreach (var item in jToken.Values())
            {
                string path = item.Path;
                this.PrepareTagNames(set, item, $"{prefix}.{(path.Contains(".") ? path.Substring(item.Path.LastIndexOf('.') + 1) : path)}");
            }
        }
    }
}