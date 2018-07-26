// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers
{
    /// <summary>This class is used to lock, write or release a document in  CosmosDB</summary>
    public class StorageWriteLock<T> where T : class, new()
    {
        private readonly string collectionId;
        private readonly string key;
        private readonly IStorageAdapterClient client;
        private readonly Action<T, bool> setLockFlagAction;
        private readonly Func<ValueApiModel, bool> testLockFunc;

        private T lastValue;
        private string lastETag;

        public StorageWriteLock(
            IStorageAdapterClient client,
            string collectionId,
            string key,
            Action<T, bool> setLockFlagAction,
            Func<ValueApiModel, bool> testLockFunc)
        {
            this.client = client;
            this.collectionId = collectionId;
            this.key = key;
            this.setLockFlagAction = setLockFlagAction;
            this.testLockFunc = testLockFunc;

            this.lastETag = null;
        }

        public async Task<bool?> TryLockAsync()
        {
            if (this.lastETag != null)
            {
                throw new ResourceOutOfDateException("Lock has already been acquired");
            }

            ValueApiModel model = null;

            try
            {
                model = await this.client.GetAsync(this.collectionId, this.key);
            }
            catch (ResourceNotFoundException)
            {
                // No need to log here since this exception is being logged on DeviceProperties.cs
            }

            if (!this.testLockFunc(model))
            {
                return false;
            }

            this.lastValue = model == null ? new T() : JsonConvert.DeserializeObject<T>(model.Data);
            this.setLockFlagAction(this.lastValue, true);

            try
            {
                this.lastETag = await this.UpdateValueAsync(this.lastValue, model?.ETag);
            }
            catch (ConflictingResourceException)
            {
                return null;
            }

            return true;
        }

        public async Task ReleaseAsync()
        {
            if (this.lastETag == null)
            {
                throw new ResourceOutOfDateException("Lock was not acquired yet");
            }

            this.setLockFlagAction(this.lastValue, false);

            try
            {
                await this.UpdateValueAsync(this.lastValue, this.lastETag);
            }
            catch (ConflictingResourceException)
            {
                // Nothing to do
            }

            this.lastETag = null;
        }

        public async Task<bool> WriteAndReleaseAsync(T newValue)
        {
            if (this.lastETag == null)
            {
                throw new ResourceOutOfDateException("Lock was not acquired yet");
            }

            this.setLockFlagAction(newValue, false);

            try
            {
                await this.UpdateValueAsync(newValue, this.lastETag);
            }
            catch (ConflictingResourceException)
            {
                return false;
            }

            this.lastETag = null;
            return true;
        }

        private async Task<string> UpdateValueAsync(T value, string etag)
        {
            var model = await this.client.UpdateAsync(
                this.collectionId,
                this.key,
                JsonConvert.SerializeObject(value),
                etag);

            return model.ETag;
        }
    }
}