// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test.TimeSeries
{
    public class TimeSeriesValueListApiModelTest
    {
        private readonly string TSI_SAMPLE_EVENTS_FILE = $"TimeSeries{Path.DirectorySeparatorChar}TimeSeriesEvents.json";

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ConvertsToMessageList_WhenMultipleDeviceTypes()
        {
            // Arrange 
            var events = this.GetTimeSeriesEvents();

            // Act
            var result = events.ToMessageList(0);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.NotEmpty(result.Properties);
            Assert.Equal(4, result.Messages.Count);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void ConvertsToMessageList_WithSkipValue()
        {
            // Arrange 
            var events = this.GetTimeSeriesEvents();

            // Act
            var result = events.ToMessageList(2);

            // Assert
            Assert.NotEmpty(result.Messages);
            Assert.NotEmpty(result.Properties);
            Assert.Equal(2, result.Messages.Count);
        }

        private ValueListApiModel GetTimeSeriesEvents()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory +
                       Path.DirectorySeparatorChar +
                       this.TSI_SAMPLE_EVENTS_FILE;

            string data = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<ValueListApiModel>(data);
        }
    }
}
