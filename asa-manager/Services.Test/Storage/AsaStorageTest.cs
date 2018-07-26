// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Moq;
using Services.Test.helpers;
using Xunit;
using Xunit.Abstractions;

namespace Services.Test.Storage
{
    public class AsaStorageTest
    {
        // Safety net: in case a unit test is spinning up threads
        // the test will be blocked after 10 seconds
        private const int TEST_TIMEOUT_MSECS = 10000;

        private readonly AsaStorage target;
        private readonly Mock<ILogger> logger;
        private readonly Mock<IFactory> factory;
        private readonly Mock<ICosmosDbSql> cosmosDbSql;

        public AsaStorageTest(ITestOutputHelper log)
        {
            this.logger = new Mock<ILogger>();
            this.factory = new Mock<IFactory>();
            this.cosmosDbSql = new Mock<ICosmosDbSql>();
            this.factory.Setup(x => x.Resolve<ICosmosDbSql>())
                .Returns(this.cosmosDbSql.Object);

            this.target = new AsaStorage(this.factory.Object, this.logger.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItRequiresInitialization()
        {
            Assert.ThrowsAsync<ApplicationException>(
                    async () => await this.target.SetupOutputStorageAsync())
                .Wait(TEST_TIMEOUT_MSECS);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItInitializesCosmosDbSqlStorage()
        {
            // Arrange
            var config = new CosmosDbTableConfiguration { ConnectionString = "foo" };
            this.target.Initialize(AsaOutputStorageType.CosmosDbSql, config);
            this.cosmosDbSql
                .Setup(x => x.Initialize(It.IsAny<CosmosDbTableConfiguration>()))
                .Returns(this.cosmosDbSql.Object);

            // Act
            this.target.SetupOutputStorageAsync().Wait(TEST_TIMEOUT_MSECS);

            // Assert
            this.cosmosDbSql.Verify(
                x => x.Initialize(It.Is<CosmosDbTableConfiguration>(c => c.ConnectionString == "foo")), Times.Once);
            this.cosmosDbSql.Verify(x => x.CreateDatabaseAndCollectionsIfNotExistAsync(), Times.Once);
        }
    }
}
