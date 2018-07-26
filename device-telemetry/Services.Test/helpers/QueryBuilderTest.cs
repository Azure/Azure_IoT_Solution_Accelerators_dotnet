// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Helpers;
using Xunit;

namespace Services.Test.helpers
{
    public class QueryBuilderTest
    {
        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetDocumentsSql_WithValidInput()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Act
            var querySpec = QueryBuilder.GetDocumentsSql(
                "alarm",
                "bef978d4-54f6-429f-bda5-db2494b833ef",
                "rule.id",
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                "asc",
                "device.msg.received",
                0,
                100,
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id");

            // Assert
            Assert.Equal($"SELECT TOP @top * FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c[@devicesProperty] IN (@devicesParameterName0,@devicesParameterName1) AND c[@byIdProperty] = @byId AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()}) ORDER BY c[@orderProperty] ASC", querySpec.QueryText);
            Assert.Equal(100, querySpec.Parameters[0].Value);
            Assert.Equal("alarm", querySpec.Parameters[1].Value);
            Assert.Equal("device.id", querySpec.Parameters[2].Value);
            Assert.Equal("chiller-01.0", querySpec.Parameters[3].Value);
            Assert.Equal("chiller-02.0", querySpec.Parameters[4].Value);
            Assert.Equal("rule.id", querySpec.Parameters[5].Value);
            Assert.Equal("bef978d4-54f6-429f-bda5-db2494b833ef", querySpec.Parameters[6].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[7].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[8].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[9].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetDocumentsSql_WithNullIdProperty()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Act
            var querySpec = QueryBuilder.GetDocumentsSql(
                "alarm",
                null,
                null,
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                "asc",
                "device.msg.received",
                0,
                100,
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id");

            // Assert
            Assert.Equal($"SELECT TOP @top * FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c[@devicesProperty] IN (@devicesParameterName0,@devicesParameterName1) AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()}) ORDER BY c[@orderProperty] ASC", querySpec.QueryText);
            Assert.Equal(100, querySpec.Parameters[0].Value);
            Assert.Equal("alarm", querySpec.Parameters[1].Value);
            Assert.Equal("device.id", querySpec.Parameters[2].Value);
            Assert.Equal("chiller-01.0", querySpec.Parameters[3].Value);
            Assert.Equal("chiller-02.0", querySpec.Parameters[4].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[5].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[6].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[7].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void FailToGetDocumentsSql_WithInvalidInput()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Assert
            Assert.Throws<InvalidInputException>(() => QueryBuilder.GetDocumentsSql(
                "alarm's",
                "bef978d4-54f6-429f-bda5-db2494b833ef",
                "rule.id",
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                "asc",
                "device.msg.received",
                0,
                100,
                new string[] { "chiller-01.0", "chiller-02.0" },
                "deviceId"));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetCountSql_WithValidInput()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Act
            var querySpec = QueryBuilder.GetCountSql(
                "alarm",
                "bef978d4-54f6-429f-bda5-db2494b833ef",
                "rule.id",
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id",
                new string[] { "open", "acknowledged" },
                "status");

            // Assert
            Assert.Equal($"SELECT VALUE COUNT(1) FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c[@devicesProperty] IN (@devicesParameterName0,@devicesParameterName1) AND c[@byIdProperty] = @byId AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()} AND c[@filterProperty] IN (@filterParameterName0,@filterParameterName1))", querySpec.QueryText);
            Assert.Equal("alarm", querySpec.Parameters[0].Value);
            Assert.Equal("device.id", querySpec.Parameters[1].Value);
            Assert.Equal("chiller-01.0", querySpec.Parameters[2].Value);
            Assert.Equal("chiller-02.0", querySpec.Parameters[3].Value);
            Assert.Equal("rule.id", querySpec.Parameters[4].Value);
            Assert.Equal("bef978d4-54f6-429f-bda5-db2494b833ef", querySpec.Parameters[5].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[6].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[7].Value);
            Assert.Equal("status", querySpec.Parameters[8].Value);
            Assert.Equal("open", querySpec.Parameters[9].Value);
            Assert.Equal("acknowledged", querySpec.Parameters[10].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void GetCountSql_WithNullIdProperty()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Act
            var querySpec = QueryBuilder.GetCountSql(
                "alarm",
                null,
                null,
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id",
                new string[] { "open", "acknowledged" },
                "status");

            // Assert
            Assert.Equal($"SELECT VALUE COUNT(1) FROM c WHERE (c[\"doc.schema\"] = @schemaName AND c[@devicesProperty] IN (@devicesParameterName0,@devicesParameterName1) AND c[@fromProperty] >= {from.ToUnixTimeMilliseconds()} AND c[@toProperty] <= {to.ToUnixTimeMilliseconds()} AND c[@filterProperty] IN (@filterParameterName0,@filterParameterName1))", querySpec.QueryText);
            Assert.Equal("alarm", querySpec.Parameters[0].Value);
            Assert.Equal("device.id", querySpec.Parameters[1].Value);
            Assert.Equal("chiller-01.0", querySpec.Parameters[2].Value);
            Assert.Equal("chiller-02.0", querySpec.Parameters[3].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[4].Value);
            Assert.Equal("device.msg.received", querySpec.Parameters[5].Value);
            Assert.Equal("status", querySpec.Parameters[6].Value);
            Assert.Equal("open", querySpec.Parameters[7].Value);
            Assert.Equal("acknowledged", querySpec.Parameters[8].Value);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void FailToGetCountSql_WithInvalidInput()
        {
            // Arrange
            var from = DateTimeOffset.Now.AddHours(-1);
            var to = DateTimeOffset.Now;

            // Assert
            Assert.Throws<InvalidInputException>(() => QueryBuilder.GetCountSql(
                "alarm",
                "'chiller-01' or 1=1",
                "rule.id",
                from,
                "device.msg.received",
                to,
                "device.msg.received",
                new string[] { "chiller-01.0", "chiller-02.0" },
                "device.id",
                new string[] { "open", "acknowledged" },
                "status"));
        }
    }
}
