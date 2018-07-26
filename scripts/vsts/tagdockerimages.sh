#!/usr/bin/env bash
APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../ && pwd )/"

declare -A microservicedockernames

microservicedockernames+=(
        ["asamanager"]="asa-manager"
        ["pcsauth"]="pcs-auth"
        ["pcsconfig"]="pcs-config"
        ["iothubmanager"]="iothub-manager"
        ["pcsstorageadapter"]="pcs-storage-adapter"
        ["devicetelemetry"]="telemetry"
        ["devicesimulation"]="device-simulation"
)

echo $SERVICESBUILT

tag()
{

    IFS=','; microservices=($SERVICESBUILT); unset IFS;
    for microservice in ${!microservices[@]}; do
        msfolder=${microservices[${microservice}]}
        echo "Folder - $msfolder"
        names=${microservicedockernames[${msfolder}]}
        echo "Names - $names"
        docker tag azureiotpcs/$names-dotnet:testing azureiotpcsdev/$names-dotnet:latest
        echo -e "azureiotpcsdev/$names-dotnet:latest" >> scripts/vsts/imagestopush
    done
}


truncate -s 0 scripts/vsts/imagestopush
tag
cat scripts/vsts/imagestopush