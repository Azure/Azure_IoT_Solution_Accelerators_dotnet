// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Models;
using Moq;
using Newtonsoft.Json.Linq;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class MessagesTest
    {
        private const int SKIP = 0;
        private const int LIMIT = 1000;

        private readonly Mock<IMessages> messages;

        public MessagesTest()
        {
            this.messages = new Mock<IMessages>();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void InitialListIsEmpty()
        {
            // Arrange
            this.ThereAreNoMessagesInStorage();

            // Act
            var list = this.messages.Object.List(null, null, null, SKIP, LIMIT, null);

            // Assert
            Assert.Equal(0, list.Messages.Count);
            Assert.Equal(0, list.Properties.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetListWithValues()
        {
            // Arrange
            this.ThereAreSomeMessagesInStorage();

            // Act
            var list = this.messages.Object.List(null, null, null, SKIP, LIMIT, null);

            // Assert
            Assert.NotEmpty(list.Messages);
            Assert.NotEmpty(list.Properties);
        }

        private void ThereAreNoMessagesInStorage()
        {
            this.messages.Setup(x => x.List(null, null, null, SKIP, LIMIT, null))
                .Returns(new MessageList());
        }

        private void ThereAreSomeMessagesInStorage()
        {
            var sampleMessages = new List<Message>();
            var sampleProperties = new List<string>();

            var data = new JObject
            {
                { "data.sample_unit", "mph" },
                { "data.sample_speed", "10" }
            };

            sampleMessages.Add(new Message("id1", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), data));
            sampleMessages.Add(new Message("id2", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), data));

            sampleProperties.Add("data.sample_unit");
            sampleProperties.Add("data.sample_speed");

            this.messages.Setup(x => x.List(null, null, null, SKIP, LIMIT, null))
                .Returns(new MessageList(sampleMessages, sampleProperties));
        }
    }
}
