:: Copyright (c) Microsoft. All rights reserved.

@ECHO off & setlocal enableextensions enabledelayedexpansion

:: Some settings are used to connect to an external dependency, e.g. Azure IoT Hub and IoT Hub Manager API
:: Depending on which settings and which dependencies are needed, edit the list of variables checked

IF "%PCS_TELEMETRY_DOCUMENTDB_CONNSTRING%" == "" (
    echo Error: the PCS_TELEMETRY_DOCUMENTDB_CONNSTRING environment variable is not defined.
    exit /B 1
)

IF "%PCS_STORAGEADAPTER_WEBSERVICE_URL%" == "" (
    echo Error: the PCS_STORAGEADAPTER_WEBSERVICE_URL environment variable is not defined.
    exit /B 1
)

IF "%PCS_AUTH_ISSUER%" == "" (
    echo Error: the PCS_AUTH_ISSUER environment variable is not defined.
    exit /B 1
)

IF "%PCS_AUTH_AUDIENCE%" == "" (
    echo Error: the PCS_AUTH_AUDIENCE environment variable is not defined.
    exit /B 1
)

endlocal
