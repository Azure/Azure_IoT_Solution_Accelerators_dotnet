// Copyright (c) Microsoft. All rights reserved.
// This function is called in the shape of 'udf.applyRuleFilter(record)'
// from ASA query. The JavaScript code snippet encoded as one line string
// in '__rulefilterjs' will be used to construct a Function callback and
// evaluated by ASA and return the result to the ASA query to filter the
// incoming record.
function main(record) {
    let ruleFunction = new Function('record', record.__rulefilterjs);
    return ruleFunction(record);
}