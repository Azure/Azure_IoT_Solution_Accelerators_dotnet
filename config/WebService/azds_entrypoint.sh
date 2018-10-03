# set env variables here
export PCS_STORAGEADAPTER_WEBSERVICE_URL=http://storageadapter/v1
export PCS_AUTH_WEBSERVICE_URL=http://auth/v1
export PCS_DEVICESIMULATION_WEBSERVICE_URL=http://simulation/v1
export PCS_TELEMETRY_WEBSERVICE_URL=http://devicetelemetry/v1
echo "Set environment variables"
env | grep PCS
echo "Run Config..."
dotnet run --no-restore --no-build --no-launch-profile