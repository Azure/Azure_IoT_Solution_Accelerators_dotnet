// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent
{
    public interface IDeviceGroupsWriter
    {
        Task ExportMapToReferenceDataAsync(
            Dictionary<string, IEnumerable<string>> deviceGroupMapping,
            DateTimeOffset timestamp);
    }

    public class DeviceGroupsWriter : IDeviceGroupsWriter
    {
        private readonly IBlobStorageHelper blobStorageHelper;
        private readonly IBlobStorageConfig blobStorageConfig;
        private readonly IFileWrapper fileWrapper;
        private readonly ILogger logger;

        public DeviceGroupsWriter(
            IBlobStorageConfig blobStorageConfig,
            IBlobStorageHelper blobStorageHelper,
            IFileWrapper fileWrapper,
            ILogger logger)
        {
            this.blobStorageConfig = blobStorageConfig;
            this.logger = logger;
            this.blobStorageHelper = blobStorageHelper;
            this.fileWrapper = fileWrapper;
        }

        /**
         * Given a dictionary of group ids to a list of device ids and a timestamp,
         * writes a csv file in the format
         * "DeviceId","GroupId"
         * "Device1","Group1"
         * ...
         * to the blob storage container and file path defined in the blob storage configuration.
         */
        public async Task ExportMapToReferenceDataAsync(
            Dictionary<string, IEnumerable<string>> deviceGroupMapping,
            DateTimeOffset timestamp)
        {
            string dateTimeFormatString = $"{this.blobStorageConfig.ReferenceDataDateFormat}/{this.blobStorageConfig.ReferenceDataTimeFormat}";
            string formattedDate = timestamp.ToString(dateTimeFormatString);
            string tempFileName = this.fileWrapper.GetTempFileName();
            try
            {
                this.WriteMappingToTemporaryFile(tempFileName, deviceGroupMapping);

                string blobName = $"{formattedDate}/{this.blobStorageConfig.ReferenceDataDeviceGroupsFileName}";

                await this.blobStorageHelper.WriteBlobFromFileAsync(blobName, tempFileName);
            }
            catch (Exception e)
            {
                this.logger.Error("Unable to create reference data", () => new { e });
            }

            try
            {
                // TODO: Better handling if file delete fails
                if (this.fileWrapper.Exists(tempFileName))
                {
                    this.fileWrapper.Delete(tempFileName);
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Unable to delete temporary reference data file", () => new { e });
            }
        }

        private void WriteMappingToTemporaryFile(
            string tempFileName,
            Dictionary<string, IEnumerable<string>> deviceGroupMapping)
        {
            using (var file = this.fileWrapper.GetStreamWriter(tempFileName))
            {
                this.fileWrapper.WriteLine(file, "\"DeviceId\",\"GroupId\"");
                foreach (string groupId in deviceGroupMapping.Keys)
                {
                    foreach (string deviceId in deviceGroupMapping[groupId])
                    {
                        this.fileWrapper.WriteLine(file, $"\"{deviceId}\",\"{groupId}\"");
                    }
                }
            }
        }
    }
}
