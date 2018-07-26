// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage
{
    // Wrapper to help with testability of cloud storage code
    public interface ICloudStorageWrapper
    {
        CloudStorageAccount Parse(string connectionString);
        CloudBlobClient CreateCloudBlobClient(CloudStorageAccount account);
        CloudBlobContainer GetContainerReference(CloudBlobClient client, string containerName);
        Task CreateIfNotExistsAsync(
            CloudBlobContainer container,
            BlobContainerPublicAccessType accessType,
            BlobRequestOptions options,
            OperationContext operationContext);

        CloudBlockBlob GetBlockBlobReference(CloudBlobContainer container, string blobName);
        Task UploadFromFileAsync(CloudBlockBlob blob, string fileName);
    }

    public class CloudStoragWrapper : ICloudStorageWrapper
    {
        public CloudBlobClient CreateCloudBlobClient(CloudStorageAccount account)
        {
            return account.CreateCloudBlobClient();
        }

        public CloudBlobContainer GetContainerReference(CloudBlobClient client, string containerName)
        {
            return client.GetContainerReference(containerName);
        }

        public Task CreateIfNotExistsAsync(CloudBlobContainer container, BlobContainerPublicAccessType accessType, BlobRequestOptions options, OperationContext operationContext)
        {
            return container.CreateIfNotExistsAsync(accessType, options, operationContext);
        }

        public CloudBlockBlob GetBlockBlobReference(CloudBlobContainer container, string blobName)
        {
            return container.GetBlockBlobReference(blobName);
        }

        public CloudStorageAccount Parse(string connectionString)
        {
            return CloudStorageAccount.Parse(connectionString);
        }

        public Task UploadFromFileAsync(CloudBlockBlob blob, string fileName)
        {
            return blob.UploadFromFileAsync(fileName);
        }
    }
}
