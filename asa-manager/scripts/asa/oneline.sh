#!/usr/bin/env bash
# Copyright (c) Microsoft. All rights reserved.
# Note: Windows Bash doesn't support shebang extra params
set -e

# Remove all comment lines from JavaScript and ASA query files
# and join the remaining lines with '\n'. The JavaScript function
# files and ASA query will be processed to generate a JSON object
# representation of ARM parameter value when no file is specified.
# The output can be copied into ARM templates to replace the default
# value of parameter 'streamingJobsQuery'.
process_files() {
	sed -e '/\/\/.*/d' -e '/--/d' $@ | sed -e ':a;N;$!ba;s/\n/\\n/g'
}

if [ "$#" -eq 0 ];
then
	applyRuleFilterJsUdf=$(process_files ./functions/applyRuleFilter.js)
	flattenMeasurementsJsUdf=$(process_files ./functions/flattenMeasurements.js)
	removeUnusedPropertiesJsUdf=$(process_files ./functions/removeUnusedProperties.js)
	transformQuery=$(process_files ./Script.asaql)
	streamingJobQuery=`cat << eof
            "defaultValue": {
                "applyRuleFilterJsUdf": "$applyRuleFilterJsUdf",
                "flattenMeasurementsJsUdf": "$flattenMeasurementsJsUdf",
                "removeUnusedPropertiesJsUdf": "$removeUnusedPropertiesJsUdf",
                "transformQuery": "$transformQuery"
            }
eof
`
echo "$streamingJobQuery"
else
	process_files $@
fi