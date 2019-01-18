
// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.RecurringTasksAgent
{
    public interface IRecurringTasksAgent
    {
        void Run();
    }

    public class Agent : IRecurringTasksAgent
    {
        // When cache initialization fails, retry in few seconds
        private const int CACHE_INIT_RETRY_SECS = 10;

        // After the cache is initialized, update it every few minutes
        private const int CACHE_UPDATE_SECS = 300;

        // When generating the cache, allow some time to finish, at least one minute
        private const int CACHE_TIMEOUT_SECS = 90;

        private readonly IDeviceProperties deviceProperties;
        private readonly ILogger log;
        private Timer cacheUpdateTimer;

        public Agent(
            IDeviceProperties deviceProperties,
            ILogger logger)
        {
            this.deviceProperties = deviceProperties;
            this.log = logger;
        }

        public void Run()
        {
            this.BuildDevicePropertiesCache();
            this.ScheduleDevicePropertiesCacheUpdate();
        }

        private void BuildDevicePropertiesCache()
        {
            while (true)
            {
                try
                {
                    this.log.Info("Creating DeviceProperties cache...", () => { });
                    this.deviceProperties.TryRecreateListAsync().Wait(CACHE_TIMEOUT_SECS * 1000);
                    this.log.Info("DeviceProperties Cache created", () => { });
                    return;
                }
                catch (Exception)
                {
                    this.log.Debug("DeviceProperties Cache creation failed, will retry in few seconds", () => new { CACHE_INIT_RETRY_SECS});
                }

                this.log.Warn("Pausing thread before retrying DeviceProperties cache creation", () => new { CACHE_INIT_RETRY_SECS });
                Thread.Sleep(CACHE_INIT_RETRY_SECS * 1000);
            }
        }

        private void ScheduleDevicePropertiesCacheUpdate()
        {
            try
            {
                this.log.Info("Scheduling a DeviceProperties cache update", () => new { CACHE_UPDATE_SECS });
                this.cacheUpdateTimer = new Timer(
                    this.UpdateDevicePropertiesCache,
                    null,
                    1000 * CACHE_UPDATE_SECS,
                    Timeout.Infinite);
                this.log.Info("DeviceProperties Cache update scheduled", () => new { CACHE_UPDATE_SECS });
            }
            catch (Exception e)
            {
                this.log.Error("DeviceProperties Cache update scheduling failed", () => new { e });
            }
        }

        private void UpdateDevicePropertiesCache(object context = null)
        {
            try
            {
                this.log.Info("Updating DeviceProperties cache...", () => { });
                this.deviceProperties.TryRecreateListAsync().Wait(CACHE_TIMEOUT_SECS * 1000);
                this.log.Info("DeviceProperties Cache updated", () => { });
            }
            catch (Exception e)
            {
                this.log.Warn("DeviceProperties Cache update failed, will retry later", () => new { CACHE_UPDATE_SECS, e });
            }

            this.ScheduleDevicePropertiesCacheUpdate();
        }
    }
}
