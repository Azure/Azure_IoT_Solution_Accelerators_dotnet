// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.External
{
    public interface IStorageAdapterClient
    {
        Task<ValueApiModel> GetAsync(string collectionId, string key);
        Task<ValueApiModel> UpdateAsync(string collectionId, string key, string value, string etag);
    }
}
