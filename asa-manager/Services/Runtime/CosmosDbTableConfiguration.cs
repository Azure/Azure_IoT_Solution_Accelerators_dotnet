// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime
{
    // CosmosDb API uses. Currently supported only "SQL".
    public enum CosmosDbApi
    {
        Sql = 1
    }
    
    // CosmosDb consistency levels
    public enum ConsistencyLevel
    {
        Strong,
        BoundedStaleness,
        Session,
        Eventual,
        ConsistentPrefix
    }
    
    public class CosmosDbTableConfiguration
    {
        public CosmosDbApi Api { get; set; }
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
        public ConsistencyLevel ConsistencyLevel { get; set; }
        public int RUs { get; set; }
    }
}
