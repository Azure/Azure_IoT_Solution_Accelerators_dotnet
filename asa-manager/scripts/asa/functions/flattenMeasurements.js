// Copyright (c) Microsoft. All rights reserved.
// This function is called in the shape of 'udf.flattenMeasurements(record)'
// from ASA query. The record will be flatten for two cases:
// 1. Aggregated measurements
//      "measurements": [
//          "measurementname": "temperature",
//          "avg": 72.0,
//          "max": 73.0,
//          "min": 71.0,
//          "count": 2,
//      ]
// will be transformed into:
//      "temperature" : {
//          "avg": 72.0,
//          "max": 73.0,
//          "min": 71.0,
//          "count": 2
//      }
//
// 2. Instant measurement:
//      { "measurementname": "temperature", "measurementvalue": "73" } 
// will be transformed into:
//      { "temperature": 73 }
function main(record) {

    let flatRecord = {
        '__deviceid': record.__deviceid,
        '__ruleid': record.__ruleid
    };

    record.measurements.forEach(function (item) {
        if (item.hasOwnProperty('measurementvalue')) {
            flatRecord[item.measurementname] = item.measurementvalue;
        }
        else {
            flatRecord[item.measurementname] = {
                'avg': item.avg,
                'max': item.max,
                'min': item.min,
                'count': item.count
            };
        }
    });

    return flatRecord;
}