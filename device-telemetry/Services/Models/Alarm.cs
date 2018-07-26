// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Documents;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class Alarm
    {
        public string ETag { get; set; }
        public string Id { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateModified { get; set; }
        public string Description { get; set; }
        public string GroupId { get; set; }
        public string DeviceId { get; set; }
        public string Status { get; set; }
        public string RuleId { get; set; }
        public string RuleSeverity { get; set; }
        public string RuleDescription { get; set; }

        public Alarm(
            string etag,
            string id,
            long dateCreated,
            long dateModified,
            string description,
            string groupId,
            string deviceId,
            string status,
            string ruleId,
            string ruleSeverity,
            string ruleDescription)
        {
            this.ETag = etag;
            this.Id = id;
            this.DateCreated = DateTimeOffset.FromUnixTimeMilliseconds(dateCreated);
            this.DateModified = DateTimeOffset.FromUnixTimeMilliseconds(dateModified);
            this.Description = description;
            this.GroupId = groupId;
            this.DeviceId = deviceId;
            this.Status = status;
            this.RuleId = ruleId;
            this.RuleSeverity = ruleSeverity;
            this.RuleDescription = ruleDescription;
        }

        public Alarm(Document doc)
        {
            if (doc != null)
            {
                this.ETag = doc.ETag;
                this.Id = doc.Id;
                this.DateCreated = DateTimeOffset.FromUnixTimeMilliseconds(doc.GetPropertyValue<long>("created"));
                this.DateModified = DateTimeOffset.FromUnixTimeMilliseconds(doc.GetPropertyValue<long>("modified"));
                this.Description = doc.GetPropertyValue<string>("description");
                this.GroupId = doc.GetPropertyValue<string>("group.id");
                this.DeviceId = doc.GetPropertyValue<string>("device.id");
                this.Status = doc.GetPropertyValue<string>("status");
                this.RuleId = doc.GetPropertyValue<string>("rule.id");
                this.RuleSeverity = doc.GetPropertyValue<string>("rule.severity");
                this.RuleDescription = doc.GetPropertyValue<string>("rule.description");
            }
        }
    }
}
