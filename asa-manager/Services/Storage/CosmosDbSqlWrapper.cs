// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage
{
    public interface ICosmosDbSqlWrapper
    {
        /// <summary>Wrap CosmosDb API for testability. Create a DB if it doesn't exist</summary>
        /// <param name="uri">CosmosDb URI</param>
        /// <param name="authKey">Authentication key</param>
        /// <param name="consistencyLevel">Consistency level</param>
        /// <param name="database">Database Id</param>
        Task CreateDatabaseIfNotExistsAsync(
            Uri uri,
            string authKey,
            ConsistencyLevel consistencyLevel,
            string database);

        /// <summary>Wrap CosmosDb API for testability. Create a collection if it doesn't exist</summary>
        /// <param name="uri">CosmosDb URI</param>
        /// <param name="authKey">Authentication key</param>
        /// <param name="consistencyLevel">Consistency level</param>
        /// <param name="database">Database Id</param>
        /// <param name="collection">Collection Id</param>
        /// <param name="RUs">Collection capacity in RUs</param>
        Task CreateDocumentCollectionIfNotExistsAsync(
            Uri uri,
            string authKey,
            ConsistencyLevel consistencyLevel,
            string database,
            string collection,
            int RUs);
    }

    public class CosmosDbSqlWrapper : ICosmosDbSqlWrapper
    {
        /// <summary>Wrap CosmosDb API for testability. Create a DB if it doesn't exist</summary>
        /// <param name="uri">CosmosDb URI</param>
        /// <param name="authKey">Authentication key</param>
        /// <param name="consistencyLevel">Consistency level</param>
        /// <param name="database">Database Id</param>
        public async Task CreateDatabaseIfNotExistsAsync(
            Uri uri,
            string authKey,
            ConsistencyLevel consistencyLevel,
            string database)
        {
            using (var client = new DocumentClient(uri, authKey, ConnectionPolicy.Default, consistencyLevel))
            {
                await client.CreateDatabaseIfNotExistsAsync(new Database { Id = database });
            }
        }

        /// <summary>Wrap CosmosDb API for testability. Create a collection if it doesn't exist</summary>
        /// <param name="uri">CosmosDb URI</param>
        /// <param name="authKey">Authentication key</param>
        /// <param name="consistencyLevel">Consistency level</param>
        /// <param name="database">Database Id</param>
        /// <param name="collection">Collection Id</param>
        /// <param name="RUs">Collection capacity in RUs</param>
        public async Task CreateDocumentCollectionIfNotExistsAsync(
            Uri uri,
            string authKey,
            ConsistencyLevel consistencyLevel,
            string database,
            string collection,
            int RUs)
        {
            using (var client = new DocumentClient(uri, authKey, ConnectionPolicy.Default, consistencyLevel))
            {
                await client.CreateDocumentCollectionIfNotExistsAsync(
                    $"/dbs/{database}",
                    new DocumentCollection { Id = collection },
                    new RequestOptions { OfferThroughput = RUs });
            }
        }
    }
}
