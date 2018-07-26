// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Moq;
using Services.Test.helpers;
using Xunit;
using Xunit.Abstractions;

namespace Services.Test.Storage
{
    public class CosmosDbSqlTest
    {
        // Safety net: in case a unit test is spinning up threads
        // the test will be blocked after 10 seconds
        private const int TEST_TIMEOUT_MSECS = 10000;

        private readonly CosmosDbSql target;
        private readonly Mock<ILogger> log;
        private readonly Mock<ICosmosDbSqlWrapper> cosmosSqlWrapper;

        public CosmosDbSqlTest(ITestOutputHelper log)
        {
            this.log = new Mock<ILogger>();
            this.cosmosSqlWrapper = new Mock<ICosmosDbSqlWrapper>();

            this.target = new CosmosDbSql(this.cosmosSqlWrapper.Object, this.log.Object);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItRequiresInitialization()
        {
            Assert.ThrowsAsync<ApplicationException>(
                    async () => await this.target.CreateDatabaseAndCollectionsIfNotExistAsync())
                .Wait(TEST_TIMEOUT_MSECS);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItValidatesTheConnectionString()
        {
            // Arrange
            var config = new CosmosDbTableConfiguration
            {
                ConnectionString = "missing"
            };
            this.target.Initialize(config);

            // Act + Assert
            Assert.ThrowsAsync<InvalidConfigurationException>(
                    async () => await this.target.CreateDatabaseAndCollectionsIfNotExistAsync())
                .Wait(TEST_TIMEOUT_MSECS);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ItCreatesDatabaseAndCollection()
        {
            // Arrange
            const string ENDPOINT = "https://localhost/";
            const string KEY = "MYKEY";
            var config = new CosmosDbTableConfiguration
            {
                ConnectionString = $"AccountEndpoint={ENDPOINT};AccountKey={KEY};",
                Database = Guid.NewGuid().ToString(),
                Collection = Guid.NewGuid().ToString(),
                ConsistencyLevel = ConsistencyLevel.Session,
                RUs = 1234
            };
            this.target.Initialize(config);

            // Act
            this.target.CreateDatabaseAndCollectionsIfNotExistAsync().Wait(TEST_TIMEOUT_MSECS);

            // Assert
            this.cosmosSqlWrapper.Verify(
                x => x.CreateDatabaseIfNotExistsAsync(
                    It.Is<Uri>(u => u.AbsoluteUri == ENDPOINT),
                    KEY,
                    Microsoft.Azure.Documents.ConsistencyLevel.Session,
                    config.Database),
                Times.Once);

            this.cosmosSqlWrapper.Verify(
                x => x.CreateDocumentCollectionIfNotExistsAsync(
                    It.Is<Uri>(u => u.AbsoluteUri == ENDPOINT),
                    KEY,
                    Microsoft.Azure.Documents.ConsistencyLevel.Session,
                    config.Database,
                    config.Collection,
                    1234),
                Times.Once);
        }
    }
}
