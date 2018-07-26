// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class DevicePropertiesTest
    {
        private Random rand = new Random();

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task GetListAsyncTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            var mockDevices = new Mock<IDevices>();

            var cache = new DeviceProperties(
                mockStorageAdapterClient.Object,
                new ServicesConfig(),
                new Logger("UnitTest", LogLevel.Debug),
                mockDevices.Object);

            var cacheValue = new DevicePropertyServiceModel
            {
                Rebuilding = false,
                Tags = new HashSet<string> { "ccc", "aaaa", "yyyy", "zzzz" },
                Reported = new HashSet<string> { "1111", "9999", "2222", "3333" }
            };

            mockStorageAdapterClient
                .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(new ValueApiModel { Data = JsonConvert.SerializeObject(cacheValue) }));

            var result = await cache.GetListAsync();
            Assert.Equal(result.Count, cacheValue.Tags.Count + cacheValue.Reported.Count);
            foreach (string tag in cacheValue.Tags)
                Assert.Contains(result, s => s.Contains(tag));
            foreach (string reported in cacheValue.Reported)
                Assert.Contains(result, s => s.Contains(reported));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task UpdateListAsyncTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            var mockDevices = new Mock<IDevices>();

            var cache = new DeviceProperties(
                mockStorageAdapterClient.Object,
                new ServicesConfig(),
                new Logger("UnitTest", LogLevel.Debug),
                mockDevices.Object);

            var oldCacheValue = new DevicePropertyServiceModel
            {
                Rebuilding = false,
                Tags = new HashSet<string> { "c", "a", "y", "z" },
                Reported = new HashSet<string> { "1", "9", "2", "3" }
            };

            var cachePatch = new DevicePropertyServiceModel
            {
                Tags = new HashSet<string> { "a", "y", "z", "@", "#" },
                Reported = new HashSet<string> { "9", "2", "3", "11", "12" }
            };

            var newCacheValue = new DevicePropertyServiceModel
            {
                Tags = new HashSet<string> { "c", "a", "y", "z", "@", "#" },
                Reported = new HashSet<string> { "1", "9", "2", "3", "12", "11" }
            };

            mockStorageAdapterClient
                .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(new ValueApiModel { Data = JsonConvert.SerializeObject(oldCacheValue) }));

            mockStorageAdapterClient
                .Setup(m => m.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(new ValueApiModel { Data = JsonConvert.SerializeObject(newCacheValue) }));

            var result = await cache.UpdateListAsync(cachePatch);

            Assert.True(result.Tags.SetEquals(newCacheValue.Tags));
            Assert.True(result.Reported.SetEquals(newCacheValue.Reported));
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task TryRecreateListAsyncSkipByTimeTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            var mockDevices = new Mock<IDevices>();

            var cache = new DeviceProperties(
                mockStorageAdapterClient.Object,
                new ServicesConfig
                {
                    DevicePropertiesTTL = 60
                },
                new Logger("UnitTest", LogLevel.Debug),
                mockDevices.Object);

            mockStorageAdapterClient
                .Setup(x => x.GetAsync(
                    It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == DeviceProperties.CACHE_KEY)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = this.rand.NextString(),
                    Data = JsonConvert.SerializeObject(new DevicePropertyServiceModel
                    {
                        Rebuilding = false,
                        Tags = new HashSet<string> { "tags.IsSimulated" }
                    }),
                    Metadata = new Dictionary<string, string>
                    {
                        { "$modified", DateTimeOffset.UtcNow.ToString(CultureInfo.InvariantCulture) }
                    }
                });

            var result = await cache.TryRecreateListAsync();
            Assert.False(result);

            mockStorageAdapterClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == DeviceProperties.CACHE_KEY)),
                    Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task TryRecreateListAsyncSkipByConflictTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            var mockDevices = new Mock<IDevices>();

            var cache = new DeviceProperties(
                mockStorageAdapterClient.Object,
                new ServicesConfig
                {
                    DevicePropertiesTTL = 10,
                    DevicePropertiesRebuildTimeout = 300
                },
                new Logger("UnitTest", LogLevel.Debug),
                mockDevices.Object);

            mockStorageAdapterClient
                .Setup(x => x.GetAsync(
                    It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == DeviceProperties.CACHE_KEY)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = this.rand.NextString(),
                    Data = JsonConvert.SerializeObject(new DevicePropertyServiceModel
                    {
                        Rebuilding = true
                    }),
                    Metadata = new Dictionary<string, string>
                    {
                        { "$modified", (DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1)).ToString(CultureInfo.InvariantCulture) }
                    }
                });

            var result = await cache.TryRecreateListAsync();
            Assert.False(result);

            mockStorageAdapterClient
                .Verify(x => x.GetAsync(
                        It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                        It.Is<string>(s => s == DeviceProperties.CACHE_KEY)),
                    Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public async Task TryRecreateListAsyncTest()
        {
            var mockStorageAdapterClient = new Mock<IStorageAdapterClient>();
            var mockDevices = new Mock<IDevices>();

            var cache = new DeviceProperties(
                mockStorageAdapterClient.Object,
                new ServicesConfig
                {
                    DevicePropertiesWhiteList = "tags.*, reported.Type, reported.Config.*",
                    DevicePropertiesTTL = 3600
                },
                new Logger("UnitTest", LogLevel.Debug),
                mockDevices.Object);

            var etagOld = this.rand.NextString();
            var etagLock = this.rand.NextString();
            var etagNew = this.rand.NextString();

            mockStorageAdapterClient
                .Setup(x => x.GetAsync(
                    It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == DeviceProperties.CACHE_KEY)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = etagOld,
                    Data = JsonConvert.SerializeObject(new DevicePropertyServiceModel
                    {
                        Rebuilding = false
                    }),
                    Metadata = new Dictionary<string, string>
                    {
                        { "$modified", (DateTimeOffset.UtcNow - TimeSpan.FromDays(1)).ToString(CultureInfo.InvariantCulture) }
                    }
                });
            mockDevices.Setup(x => x.GetDeviceTwinNamesAsync())
                .ReturnsAsync(new DeviceTwinName
                {
                    Tags = new HashSet<string> { "Building", "Group" },
                    ReportedProperties = new HashSet<string> { "Config.Interval", "otherProperty" }
                });

            mockStorageAdapterClient
                .Setup(x => x.UpdateAsync(
                    It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == DeviceProperties.CACHE_KEY),
                    It.Is<string>(s => Rebuilding(s)),
                    It.Is<string>(s => s == etagOld)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = etagLock
                });

            mockStorageAdapterClient
                .Setup(x => x.UpdateAsync(
                    It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == DeviceProperties.CACHE_KEY),
                    It.Is<string>(s => !Rebuilding(s)),
                    It.Is<string>(s => s == etagLock)))
                .ReturnsAsync(new ValueApiModel
                {
                    ETag = etagNew
                });

            var expiredNames = new DeviceTwinName
            {
                Tags = new HashSet<string>
                {
                    "Building", "Group"
                },
                ReportedProperties = new HashSet<string>
                {
                    "Type", "Config.Interval", "MethodStatus", "UpdateStatus"
                }
            };

            var result = await cache.TryRecreateListAsync();
            Assert.True(result);

            mockStorageAdapterClient
                .Verify(x => x.GetAsync(
                    It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == DeviceProperties.CACHE_KEY)),
                    Times.Once);

            mockDevices
                .Verify(x => x.GetDeviceTwinNamesAsync(), Times.Once);

            mockStorageAdapterClient
                .Verify(x => x.UpdateAsync(
                    It.Is<string>(s => s == DeviceProperties.CACHE_COLLECTION_ID),
                    It.Is<string>(s => s == DeviceProperties.CACHE_KEY),
                    It.Is<string>(s => Rebuilding(s)),
                    It.Is<string>(s => s == etagOld)),
                    Times.Once);
        }

        private static bool Rebuilding(string data)
        {
            return JsonConvert.DeserializeObject<DevicePropertyServiceModel>(data).Rebuilding;
        }

        private static bool CheckNames(string data, DeviceTwinName expiredNames)
        {
            var cacheValue = JsonConvert.DeserializeObject<DevicePropertyServiceModel>(data);
            return cacheValue.Tags.SetEquals(expiredNames.Tags)
                   && cacheValue.Reported.SetEquals(expiredNames.ReportedProperties);
        }
    }
}
