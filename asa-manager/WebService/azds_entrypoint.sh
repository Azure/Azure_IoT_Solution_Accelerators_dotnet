#set env variables 
export PCS_IOTHUBMANAGER_WEBSERVICE_URL=http://iothubmanager/v1
export PCS_CONFIG_WEBSERVICE_URL=http://config/v1
export PCS_TELEMETRY_WEBSERVICE_URL=http://devicetelemetry/v1
export PCS_AUTH_WEBSERVICE_URL=http://auth/v1
echo "Set environment variables"
env | grep PCS
echo "Run ASA manager..."
sleep 20
dotnet run --no-restore --no-build --no-launch-profile