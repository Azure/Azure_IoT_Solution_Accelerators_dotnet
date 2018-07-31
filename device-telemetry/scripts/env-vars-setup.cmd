:: Copyright (c) Microsoft. All rights reserved.

:: Prepare the environment variables used by the application.

:: Some settings are used to connect to an external dependency, e.g. Azure IoT Hub and IoT Hub Manager API
:: Depending on which settings and which dependencies are needed, edit the list of variables

:: see: Shared access policies => key name => Connection string
SETX PCS_TELEMETRY_DOCUMENTDB_CONNSTRING "..."

:: The URL where IoT Hub Manager web service is listening
SETX PCS_STORAGEADAPTER_WEBSERVICE_URL "http://127.0.0.1:9022/v1"

:: Endpoint to reach the authentication service
SETX PCS_AUTH_WEBSERVICE_URL "http://127.0.0.1:9001/v1"

:: The OpenId tokens issuer URL, e.g. https://sts.windows.net/12000000-3400-5600-0000-780000000000/
SETX PCS_AUTH_ISSUER "{enter the token issuer URL here}"

:: The intended audience of the tokens, e.g. your Client Id
SETX PCS_AUTH_AUDIENCE "{enter the tokens audience here}"
