// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IDeviceProperties
    {
        Task<List<string>> GetListAsync();

        Task<DevicePropertyServiceModel> UpdateListAsync(
            DevicePropertyServiceModel devicePropertyServiceModel);

        Task<bool> TryRecreateListAsync(bool force = false);
    }

    /// <summary>  
    /// This class creates/reads cache of deviceProperties in/from CosmosDB.
    /// </summary> 
    /// <remarks>
    /// This is done to avoid request throttling when deviceProperties are queried directly from IOT-Hub.
    /// This class is called "deviceProperties" even though it deals with both properties
    /// and tags of devices.
    /// </remarks>
    public class DeviceProperties : IDeviceProperties
    {
        private readonly IStorageAdapterClient storageClient;
        private readonly IDevices devices;
        private readonly ILogger log;
        /// Hardcoded in appsettings.ini
        private readonly string whitelist;
        /// Hardcoded in appsettings.ini
        private readonly long ttl;
        /// Hardcoded in appsettings.ini
        private readonly long rebuildTimeout;
        private readonly TimeSpan serviceQueryInterval = TimeSpan.FromSeconds(10);
        internal const string CACHE_COLLECTION_ID = "device-twin-properties";
        internal const string CACHE_KEY = "cache";

        private const string WHITELIST_TAG_PREFIX = "tags.";
        private const string WHITELIST_REPORTED_PREFIX = "reported.";
        private const string TAG_PREFIX = "Tags.";
        private const string REPORTED_PREFIX = "Properties.Reported.";

        /// <summary>
        /// The constructor.
        /// </summary>
        public DeviceProperties(IStorageAdapterClient storageClient,
            IServicesConfig config,
            ILogger logger,
            IDevices devices)
        {
            this.storageClient = storageClient;
            this.log = logger;
            this.whitelist = config.DevicePropertiesWhiteList;
            this.ttl = config.DevicePropertiesTTL;
            this.rebuildTimeout = config.DevicePropertiesRebuildTimeout;
            this.devices = devices;
        }

        /// <summary>
        /// Get List of deviceProperties from cache
        /// </summary>
        public async Task<List<string>> GetListAsync()
        {
            ValueApiModel response = new ValueApiModel();
            try
            {
                response = await this.storageClient.GetAsync(CACHE_COLLECTION_ID, CACHE_KEY);
            }
            catch (ResourceNotFoundException)
            {
                this.log.Debug($"Cache get: cache {CACHE_COLLECTION_ID}:{CACHE_KEY} was not found",
                    () => { });
            }
            catch (Exception e)
            {
                throw new ExternalDependencyException(
                    $"Cache get: unable to get device-twin-properties cache", e);
            }

            DevicePropertyServiceModel properties = new DevicePropertyServiceModel();
            try
            {
                properties = JsonConvert.DeserializeObject<DevicePropertyServiceModel>(response.Data);
            }
            catch (Exception e)
            {
                throw new InvalidInputException("Unable to deserialize deviceProperties from CosmosDB", e);
            }
            List<string> result = new List<string>();
            foreach (string tag in properties.Tags)
            {
                result.Add(TAG_PREFIX + tag);
            }
            foreach (string reported in properties.Reported)
            {
                result.Add(REPORTED_PREFIX + reported);
            }
            return result;
        }

        /// <summary>
        /// Try to create cache of deviceProperties if lock failed retry after 10 seconds
        /// </summary>
        public async Task<bool> TryRecreateListAsync(bool force = false)
        {
            var @lock = new StorageWriteLock<DevicePropertyServiceModel>(
                this.storageClient,
                CACHE_COLLECTION_ID,
                CACHE_KEY,
                (c, b) => c.Rebuilding = b,
                m => this.ShouldCacheRebuild(force, m));

            while (true)
            {
                var locked = await @lock.TryLockAsync();
                if (locked == null)
                {
                    this.log.Warn("Cache rebuilding: lock failed due to conflict. Retry soon", () => { });
                    continue;
                }

                if (!locked.Value)
                {
                    return false;
                }

                // Build the cache content
                var twinNamesTask = this.GetValidNamesAsync();

                try
                {
                    Task.WaitAll(twinNamesTask);
                }
                catch (Exception)
                {
                    this.log.Warn(
                        $"Some underlying service is not ready. Retry after {this.serviceQueryInterval}",
                        () => { });
                    try
                    {
                        await @lock.ReleaseAsync();
                    }
                    catch (Exception e)
                    {
                        log.Error("Cache rebuilding: Unable to release lock", () => e);
                    }
                    await Task.Delay(this.serviceQueryInterval);
                    continue;
                }

                var twinNames = twinNamesTask.Result;
                try
                {
                    var updated = await @lock.WriteAndReleaseAsync(
                        new DevicePropertyServiceModel
                        {
                            Tags = twinNames.Tags,
                            Reported = twinNames.ReportedProperties
                        });
                    if (updated)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    log.Error("Cache rebuilding: Unable to write and release lock", () => e);
                }
                this.log.Warn("Cache rebuilding: write failed due to conflict. Retry soon", () => { });
            }
        }

        /// <summary>
        /// Update Cache when devices are modified/created
        /// </summary>
        public async Task<DevicePropertyServiceModel> UpdateListAsync(
            DevicePropertyServiceModel deviceProperties)
        {
            // To simplify code, use empty set to replace null set
            deviceProperties.Tags = deviceProperties.Tags ?? new HashSet<string>();
            deviceProperties.Reported = deviceProperties.Reported ?? new HashSet<string>();

            string etag = null;
            while (true)
            {
                ValueApiModel model = null;
                try
                {
                    model = await this.storageClient.GetAsync(CACHE_COLLECTION_ID, CACHE_KEY);
                }
                catch (ResourceNotFoundException)
                {
                    this.log.Info($"Cache updating: cache {CACHE_COLLECTION_ID}:{CACHE_KEY} was not found",
                        () => { });
                }

                if (model != null)
                {
                    DevicePropertyServiceModel devicePropertiesFromStorage;

                    try
                    {
                        devicePropertiesFromStorage = JsonConvert.
                            DeserializeObject<DevicePropertyServiceModel>(model.Data);
                    }
                    catch
                    {
                        devicePropertiesFromStorage = new DevicePropertyServiceModel();
                    }
                    devicePropertiesFromStorage.Tags = devicePropertiesFromStorage.Tags ??
                        new HashSet<string>();
                    devicePropertiesFromStorage.Reported = devicePropertiesFromStorage.Reported ??
                        new HashSet<string>();

                    deviceProperties.Tags.UnionWith(devicePropertiesFromStorage.Tags);
                    deviceProperties.Reported.UnionWith(devicePropertiesFromStorage.Reported);
                    etag = model.ETag;
                    // If the new set of deviceProperties are already there in cache, return
                    if (deviceProperties.Tags.Count == devicePropertiesFromStorage.Tags.Count &&
                        deviceProperties.Reported.Count == devicePropertiesFromStorage.Reported.Count)
                    {
                        return deviceProperties;
                    }
                }

                var value = JsonConvert.SerializeObject(deviceProperties);
                try
                {
                    var response = await this.storageClient.UpdateAsync(
                        CACHE_COLLECTION_ID, CACHE_KEY, value, etag);
                    return JsonConvert.DeserializeObject<DevicePropertyServiceModel>(response.Data);
                }
                catch (ConflictingResourceException)
                {
                    this.log.Info("Cache updating: failed due to conflict. Retry soon", () => { });
                }
                catch (Exception e)
                {
                    this.log.Info("Cache updating: failed", () => e);
                    throw new Exception("Cache updating: failed");
                }
            }
        }

        /// <summary>
        /// Get list of DeviceTwinNames from IOT-hub and whitelist it.
        /// </summary>
        /// <remarks>
        /// List of Twin Names to be whitelisted is hardcoded in appsettings.ini
        /// </remarks>
        private async Task<DeviceTwinName> GetValidNamesAsync()
        {
            ParseWhitelist(this.whitelist, out var fullNameWhitelist, out var prefixWhitelist);

            var validNames = new DeviceTwinName
            {
                Tags = fullNameWhitelist.Tags,
                ReportedProperties = fullNameWhitelist.ReportedProperties
            };

            if (prefixWhitelist.Tags.Any() || prefixWhitelist.ReportedProperties.Any())
            {
                DeviceTwinName allNames = new DeviceTwinName();
                try
                {
                    /// Get list of DeviceTwinNames from IOT-hub
                    allNames = await this.devices.GetDeviceTwinNamesAsync();
                }
                catch (Exception e)
                {
                    throw new ExternalDependencyException("Unable to fetch IoT devices", e);
                }
                validNames.Tags.UnionWith(allNames.Tags.
                    Where(s => prefixWhitelist.Tags.Any(s.StartsWith)));

                validNames.ReportedProperties.UnionWith(
                    allNames.ReportedProperties.Where(
                        s => prefixWhitelist.ReportedProperties.Any(s.StartsWith)));
            }

            return validNames;
        }

        /// <summary>
        /// Parse the comma seperated string "whitelist" and create two separate list
        /// One with regex(*) and one without regex(*)
        /// </summary>
        /// <param name="whitelist">Comma seperated list of deviceTwinName to be 
        /// whitlisted which is hardcoded in appsettings.ini.</param>
        /// <param name="fullNameWhitelist">An out paramenter which is a list of
        /// deviceTwinName to be whitlisted without regex.</param>
        /// <param name="prefixWhitelist">An out paramenter which is a list of 
        /// deviceTwinName to be whitlisted with regex.</param>
        private static void ParseWhitelist(string whitelist,
            out DeviceTwinName fullNameWhitelist,
            out DeviceTwinName prefixWhitelist)
        {
            /// <example>
            /// whitelist = "tags.*, reported.Protocol, reported.SupportedMethods,
            ///                 reported.DeviceMethodStatus, reported.FirmwareUpdateStatus"
            /// whitelistItems = [tags.*,
            ///                   reported.Protocol,
            ///                   reported.SupportedMethods,
            ///                   reported.DeviceMethodStatus,
            ///                   reported.FirmwareUpdateStatus]
            /// </example>
            var whitelistItems = whitelist.Split(',').Select(s => s.Trim());

            /// <example>
            /// tags = [tags.*]
            /// </example>
            var tags = whitelistItems
                .Where(s => s.StartsWith(WHITELIST_TAG_PREFIX, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Substring(WHITELIST_TAG_PREFIX.Length));

            /// <example>
            /// reported = [reported.Protocol,
            ///             reported.SupportedMethods,
            ///             reported.DeviceMethodStatus,
            ///             reported.FirmwareUpdateStatus]
            /// </example>
            var reported = whitelistItems
                .Where(s => s.StartsWith(WHITELIST_REPORTED_PREFIX, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Substring(WHITELIST_REPORTED_PREFIX.Length));

            /// <example>
            /// fixedTags = []
            /// </example>
            var fixedTags = tags.Where(s => !s.EndsWith("*"));
            /// <example>
            /// fixedReported = [reported.Protocol,
            ///                  reported.SupportedMethods,
            ///                  reported.DeviceMethodStatus,
            ///                  reported.FirmwareUpdateStatus]
            /// </example>
            var fixedReported = reported.Where(s => !s.EndsWith("*"));

            /// <example>
            /// regexTags = [tags.]
            /// </example>
            var regexTags = tags.Where(s => s.EndsWith("*")).Select(s => s.Substring(0, s.Length - 1));
            /// <example>
            /// regexReported = []
            /// </example>
            var regexReported = reported.
                Where(s => s.EndsWith("*")).
                Select(s => s.Substring(0, s.Length - 1));

            /// <example>
            /// fullNameWhitelist = {Tags = [],
            ///                      ReportedProperties = [
            ///                         reported.Protocol, 
            ///                         reported.SupportedMethods,
            ///                         reported.DeviceMethodStatus,
            ///                         reported.FirmwareUpdateStatus]
            ///                      }
            /// </example>
            fullNameWhitelist = new DeviceTwinName
            {
                Tags = new HashSet<string>(fixedTags),
                ReportedProperties = new HashSet<string>(fixedReported)
            };

            /// <example>
            /// prefixWhitelist = {Tags = [tags.],
            ///                    ReportedProperties = []}
            /// </example>
            prefixWhitelist = new DeviceTwinName
            {
                Tags = new HashSet<string>(regexTags),
                ReportedProperties = new HashSet<string>(regexReported)
            };
        }

        /// <summary>
        /// A function to decide whether or not cache needs to be rebuilt based on force flag and existing
        /// cache's validity
        /// </summary>
        /// <param name="force">A boolean flag to decide if cache needs to be rebuilt.</param>
        /// <param name="valueApiModel">An existing valueApiModel to check whether or not cache 
        /// has expired</param>
        private bool ShouldCacheRebuild(bool force, ValueApiModel valueApiModel)
        {
            if (force)
            {
                this.log.Info("Cache will be rebuilt due to the force flag", () => { });
                return true;
            }

            if (valueApiModel == null)
            {
                this.log.Info("Cache will be rebuilt since no cache was found", () => { });
                return true;
            }
            DevicePropertyServiceModel cacheValue = new DevicePropertyServiceModel();
            DateTimeOffset timstamp = new DateTimeOffset();
            try
            {
                cacheValue = JsonConvert.DeserializeObject<DevicePropertyServiceModel>(valueApiModel.Data);
                timstamp = DateTimeOffset.Parse(valueApiModel.Metadata["$modified"]);
            }
            catch
            {
                this.log.Info("DeviceProperties will be rebuilt because the last one is broken.", () => { });
                return true;
            }

            if (cacheValue.Rebuilding)
            {
                if (timstamp.AddSeconds(this.rebuildTimeout) < DateTimeOffset.UtcNow)
                {
                    this.log.Debug("Cache will be rebuilt because last rebuilding had timedout", () => { });
                    return true;
                }
                else
                {
                    this.log.Debug
                        ("Cache rebuilding skipped because it is being rebuilt by other instance", () => { });
                    return false;
                }
            }
            else
            {
                if (cacheValue.IsNullOrEmpty())
                {
                    this.log.Info("Cache will be rebuilt since it is empty", () => { });
                    return true;
                }

                if (timstamp.AddSeconds(this.ttl) < DateTimeOffset.UtcNow)
                {
                    this.log.Info("Cache will be rebuilt because it has expired", () => { });
                    return true;
                }
                else
                {
                    this.log.Debug("Cache rebuilding skipped because it has not expired", () => { });
                    return false;
                }
            }
        }
    }
}
