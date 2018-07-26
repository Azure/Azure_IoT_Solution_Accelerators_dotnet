:: Copyright (c) Microsoft. All rights reserved.

::  Prepare the environment variables used by the application.
::

:: Endpoint used to retrieve the list of monitoring rules
SETX PCS_TELEMETRY_WEBSERVICE_URL "http://127.0.0.1:9004/v1"

:: Endpoint used to retrieve the list of device groups
SETX PCS_CONFIG_WEBSERVICE_URL "http://127.0.0.1:9005/v1"

:: Endpoint used to retrieve the list of devices in each group
SETX PCS_IOTHUBMANAGER_WEBSERVICE_URL "http://127.0.0.1:9002/v1"

:: Connection details of the Azure Blob where event hub checkpoints and reference data are stored
SETX PCS_ASA_DATA_AZUREBLOB_ACCOUNT "..."
SETX PCS_ASA_DATA_AZUREBLOB_KEY "..."
SETX PCS_ASA_DATA_AZUREBLOB_ENDPOINT_SUFFIX "..."

:: Event Hub connections string for where device notifications are stored
:: IotHub needs to be set up to send device twin and lifecycle messages to this event hub, see below links
:: https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-device-twins
:: https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-identity-registry
SETX PCS_EVENTHUB_CONNSTRING "Endpoint=sb://....servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=..."
:: Name of event hub where device notifications are stored
SETX PCS_EVENTHUB_NAME "..."

:: Azure CosmosDb SQL connection string, storage used for telemetry and alarms
SETX PCS_TELEMETRY_DOCUMENTDB_CONNSTRING "AccountEndpoint=https://....documents.azure.com:443/;AccountKey=...;"
