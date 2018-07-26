#!/usr/bin/env bash
APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../ && pwd )/"

servicestobuild=$SERVICESTOBUILD
declare -A microservicefolders
 servicesbuilt=""

microservicefolders+=(
        ["asamanager"]="asa-manager"
        ["pcsauth"]="pcs-auth"
        ["pcsconfig"]="pcs-config"
        ["iothubmanager"]="iothub-manager"
        ["pcsstorageadapter"]="pcs-storage-adapter"
        ["devicetelemetry"]="device-telemetry"
        ["devicesimulation"]="device-simulation"
)

set_env_vars_for_build() 
{
    servicesbuilt="${servicesbuilt%\,}"
    servicesbuilt="${servicesbuilt#\,}"
    echo "##vso[task.setvariable variable=servicesbuilt]$servicesbuilt"
}

build()
{

    IFS=','; microservices=($servicestobuild); unset IFS;
    for microservice in ${!microservices[@]}; do
        msfolder=${microservices[${microservice}]}
        location=${microservicefolders[${msfolder}]}
        cd $location
        scripts/docker/build

        if [ $? -eq 0 ]; then
             servicesbuilt="$servicesbuilt,$msfolder"
        fi
        cd ..
    done
}

build
set_env_vars_for_build