#set env vars
export PCS_STORAGEADAPTER_WEBSERVICE_URL=http://storageadapter/v1
export PCS_AUTH_WEBSERVICE_URL=http://auth/v1/
echo "Set environment variables"
env | grep PCS
echo "Run Telemetry manager..."
dotnet run --no-restore --no-build --no-launch-profile