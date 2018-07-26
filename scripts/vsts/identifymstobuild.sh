#!/usr/bin/env bash
# Copyright (c) Microsoft. All rights reserved.
# Note: Windows Bash doesn't support shebang extra params

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../ && pwd )/"


changes="";
servicestobuild=""
declare -A microservices

microservices+=(
        ["asamanager"]="asa\-manager\/"
        ["pcsauth"]="pcs\-auth\/"
        ["pcsconfig"]="pcs\-config\/"
        ["iothubmanager"]="iothub\-manager\/"
        ["pcsstorageadapter"]="pcs\-storage\-adapter\/"
        ["devicetelemetry"]="device\-telemetry\/"
        ["devicesimulation"]="device\-simulation\/"
)

get_changed_folders() 
{
    if [ "$BUILD_SOURCEBRANCHNAME" == "master" ]; then
        changes=$(git log --name-only --oneline -1 --pretty="format:")
     else
        changes=$(git whatchanged --name-only --pretty="" origin/master..HEAD)
     fi
     echo $changes
}

check_if_microservice_changed() 
{
    ifchanged=$(echo $changes | grep $1)
    if [[ "$ifchanged" != "" ]]; then
         servicestobuild="$servicestobuild,$2"
    fi
}

set_env_vars_for_build() 
{
    servicestobuild="${servicestobuild%\,}"
    servicestobuild="${servicestobuild#\,}"
    echo "##vso[task.setvariable variable=servicestobuild]$servicestobuild"
}

main()
{
    get_changed_folders
    for microservice in ${!microservices[@]}; 
    do
        regex=${microservices[${microservice}]}
        check_if_microservice_changed $regex $microservice
    done
    set_env_vars_for_build 
}

main
echo $servicestobuild