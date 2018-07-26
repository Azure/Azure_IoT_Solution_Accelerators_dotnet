// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage
{
    public interface ICosmosDbSql
    {
        // Initialize the instance, required before running any other method
        ICosmosDbSql Initialize(CosmosDbTableConfiguration config);
        
        // Create database and collection if required
        Task CreateDatabaseAndCollectionsIfNotExistAsync();
    }

    public class CosmosDbSql : ICosmosDbSql
    {
        // Pattern used to extract URI and Auth key from the connection string
        private const string CONNSTRING_FORMAT = "^AccountEndpoint=(?<endpoint>.*);AccountKey=(?<key>.*);$";

        // Wrap SDK methods for testability
        private readonly ICosmosDbSqlWrapper cosmosSqlWrapper;
        private readonly ILogger log;

        private string connectionString;
        private ConsistencyLevel consistencyLevel;
        private string database;
        private string collection;
        private int RUs;
        private bool initialized;

        public CosmosDbSql(
            ICosmosDbSqlWrapper cosmosSqlWrapper,
            ILogger logger)
        {
            this.cosmosSqlWrapper = cosmosSqlWrapper;
            this.log = logger;
            this.initialized = false;
        }

        // Initialize the instance, required before running any other method
        // Returns the instance so that the method can be chained to the ctor.
        public ICosmosDbSql Initialize(CosmosDbTableConfiguration config)
        {
            this.connectionString = config.ConnectionString;
            this.consistencyLevel = config.ConsistencyLevel;
            this.database = config.Database;
            this.collection = config.Collection;
            this.RUs = config.RUs;
            this.initialized = true;
            return this;
        }

        // Create database and collection if required
        public async Task CreateDatabaseAndCollectionsIfNotExistAsync()
        {
            if (!this.initialized)
            {
                // Note: this is an application bug
                this.log.Error("Initialize() not invoked yet.", () => { });
                throw new ApplicationException("Initialize() not invoked yet.");
            }

            this.ParseConnectionString(out var uri, out var authKey);
            var cLevel = GetConsistencyLevel(this.consistencyLevel);

            await this.cosmosSqlWrapper.CreateDatabaseIfNotExistsAsync(uri, authKey, cLevel, this.database);
            await this.cosmosSqlWrapper.CreateDocumentCollectionIfNotExistsAsync(uri, authKey, cLevel, this.database, this.collection, this.RUs);
        }

        // Parse the connection string and extract URI and Auth Key
        private void ParseConnectionString(out Uri uri, out string authKey)
        {
            var match = Regex.Match(this.connectionString, CONNSTRING_FORMAT);
            if (!match.Success)
            {
                // Note: do not log the connection string to avoid secrets ending in the logs
                this.log.Error("Invalid connection string for Cosmos DB", () => { });
                throw new InvalidConfigurationException($"Invalid connection string for Cosmos DB (format: '${CONNSTRING_FORMAT}')");
            }

            uri = new Uri(match.Groups["endpoint"].Value);
            authKey = match.Groups["key"].Value;
        }

        // Simple mapper to the enum used by CosmosDb SDK
        private static Documents.ConsistencyLevel GetConsistencyLevel(ConsistencyLevel v)
        {
            switch (v)
            {
                case ConsistencyLevel.Strong:
                    return Documents.ConsistencyLevel.Strong;

                case ConsistencyLevel.BoundedStaleness:
                    return Documents.ConsistencyLevel.BoundedStaleness;

                case ConsistencyLevel.Session:
                    return Documents.ConsistencyLevel.Session;

                case ConsistencyLevel.Eventual:
                    return Documents.ConsistencyLevel.Eventual;

                case ConsistencyLevel.ConsistentPrefix:
                    return Documents.ConsistencyLevel.ConsistentPrefix;

                default:
                    throw new ArgumentOutOfRangeException("consistency level", v, "Unknown consistency level");
            }
        }
    }
}
