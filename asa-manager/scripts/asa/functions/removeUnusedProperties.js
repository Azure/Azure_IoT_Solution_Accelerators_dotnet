// Copyright (c) Microsoft. All rights reserved.
// This function is called in the shape of 'udf.removeUnusedProperties(record)'
// from ASA query. Those unused properties will be removed from record for the
// next step of ASA query.
function main(record) {
    if (record) {
        record.IoTHub && delete record.IoTHub;
        record.PartitionId && delete record.PartitionId;
        record.EventEnqueuedUtcTime && delete record.EventEnqueuedUtcTime;
        record.EventProcessedUtcTime && delete record.EventProcessedUtcTime;
    }
    return record;
}