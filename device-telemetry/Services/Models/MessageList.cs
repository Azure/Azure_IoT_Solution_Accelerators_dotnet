// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models
{
    public class MessageList
    {
        public List<Message> Messages { get; set; }
        public List<string> Properties { get; set; }

        public MessageList()
        {
            this.Messages = new List<Message>();
            this.Properties = new List<string>();
        }

        public MessageList(
            List<Message> messages,
            List<string> properties)
        {
            if (messages != null) this.Messages = messages;
            if (properties != null) this.Properties = properties;
        }
    }
}
