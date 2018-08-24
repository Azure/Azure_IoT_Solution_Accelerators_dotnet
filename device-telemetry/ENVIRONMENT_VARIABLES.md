# Environment Variables

The service requires some required environment settings and supports some
customizations via optional environment variables.

Depending on the OS and how the microservice is run, environment variables
can be defined in multiple ways. See below how to set environment variables
in the different contexts.

## Required Settings

* `PCS_TELEMETRY_DOCUMENTDB_CONNSTRING` [required]: contains the full connection
  string required to connect the telemetry to the Cosmos DB in the Azure cloud, 
  where the Rules, Messages and the Alarms are stored.

* `PCS_STORAGEADAPTER_WEBSERVICE_URL` [required]: the URL where the storage 
  adapter service is available, e.g. `http://127.0.0.1:9022`

* `PCS_EVENTHUB_CONNSTRING`: [required]:contains the full connection 
  string required to connect the service to the EventHub namespace.
  * EventHub Info can be found in the Azure portal at:
    {Your EventHub} > Shared access policies > RootManageSharedAccessKey

* `PCS_EVENTHUB_NAME`: [required]: the name of the eventhub used 
  for notification system in the telemetry service.
  * EventHub Info can be found in the Azure portal at:
    {Your EventHub} > Shared access policies > RootManageSharedAccessKey

* `PCS_ASA_DATA_AZUREBLOB_ACCOUNT`: [required]: blob storage account 
  used for storing checkpointing data for the eventhub used in the notification system.
  * Storage Account information:
    {Your storage account} > Access keys

* `PCS_ASA_DATA_AZUREBLOB_KEY`: [required]: blob storage key used 
  for storing checkpointing data for the eventhub used in the notification system.
  * Storage Account information:
    {Your storage account} > Access keys

* `PCS_TELEMETRY_LOGICAPP_ENDPOINT_URL`: [required]: logic app end point
  URL used to send email actions for Notification system.

* `PCS_SOLUTION_NAME`: [required]: Solution name used to generate URL
  for email notifications.
  * This should be the resource group name for your deployment.

## Optional Settings

* `PCS_AUTH_REQUIRED` [optional, default `True`]: whether the web service requires client authentication, e.g. via the Authorization headers.

* `PCS_CORS_WHITELIST` [optional, default empty]: the web service cross-origin request settings. By default, cross origin requests are not allowed.
  Use `{ 'origins': ['*'], 'methods': ['*'], 'headers': ['*'] }` to allow any request during development.  In Production CORS is not required, and should be used very carefully if enabled.

* `PCS_AUTH_ISSUER` [optional, default empty]: the OAuth2 JWT tokens issuer, e.g. `https://sts.windows.net/fa01ade2-2365-4dd1-a084-a6ef027090fc/`.

* `PCS_AUTH_AUDIENCE` [optional, default empty]: the OAuth2 JWT tokens audience, e.g. `2814e709-6a0e-4861-9594-d3b6e2b81331`.

* `PCS_AUTH_WEBSERVICE_URL`[optional, default empty]: The URL where the auth service is available, e.g. `http://localhost:9001/v1`
