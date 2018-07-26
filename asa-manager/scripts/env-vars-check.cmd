:: Copyright (c) Microsoft. All rights reserved.

@ECHO off & setlocal enableextensions enabledelayedexpansion

IF "%PCS_TELEMETRY_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_TELEMETRY_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_CONFIG_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_CONFIG_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_IOTHUBMANAGER_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_IOTHUBMANAGER_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_ASA_DATA_AZUREBLOB_ACCOUNT%" == "" (
    echo Error: the PCS_ASA_DATA_AZUREBLOB_ACCOUNT environment variable is not defined.
    exit /B 1
)

IF "%PCS_ASA_DATA_AZUREBLOB_KEY%" == "" (
    echo Error: the PCS_ASA_DATA_AZUREBLOB_KEY environment variable is not defined.
    exit /B 1
)

IF "%PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX%" == "" (
    echo Error: the PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX environment variable is not defined.
    exit /B 1
)

IF "%PCS_EVENTHUB_CONNSTRING%" == "" (
    echo Error: the PCS_EVENTHUB_CONNSTRING environment variable is not defined.
    exit /B 1
)

IF "%PCS_TELEMETRY_DOCUMENTDB_CONNSTRING%" == "" (
    echo Error: the PCS_TELEMETRY_DOCUMENTDB_CONNSTRING environment variable is not defined.
    exit /B 1
)

IF "%PCS_EVENTHUB_NAME%" == "" (
    echo Error: the PCS_EVENTHUB_NAME environment variable is not defined.
    exit /B 1
)

endlocal
