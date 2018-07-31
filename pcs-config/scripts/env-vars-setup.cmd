:: Prepare the environment variables used by the application

:: Endpoint to reach the storage adapter
SETX PCS_STORAGEADAPTER_WEBSERVICE_URL "http://localhost:9022/v1"

:: Endpoint to reach the telemetry
SETX PCS_TELEMETRY_WEBSERVICE_URL "http://localhost:9004/v1"

:: Endpoint to reach the device simlation
SETX PCS_DEVICESIMULATION_WEBSERVICE_URL "http://localhost:9003/v1"

:: Azure Maps API Key
SETX PCS_AZUREMAPS_KEY "static"

:: Endpoint to reach the authentication service
SETX PCS_AUTH_WEBSERVICE_URL "http://localhost:9001/v1"
