#!/bin/bash
set -e

declare -A microservicefolders

microservicefolders+=(
         ["asamanager"]="asa-manager"
         ["pcsauth"]="auth"
         ["pcsconfig"]="config"
         ["iothubmanager"]="iothub-manager"
         ["pcsstorageadapter"]="storage-adapter"
         ["devicetelemetry"]="device-telemetry"
         ["devicesimulation"]="device-simulation"
)



function clone_repos {
        git clone https://github.com/Azure/remote-monitoring-services-dotnet
        git clone https://github.com/Azure/device-simulation-dotnet
}

function checkout_and_merge_master {
        git checkout azure_dev_spaces
        git merge master
        CONFLICTS=$(git ls-files -u | wc -l)
        if [ "$CONFLICTS" -gt 0 ] ; then
               echo "There is a merge conflict. Aborting"
               git merge --abort
               exit 1
        fi

}

#TODO: Push changes when merge is clean
function git_push {
        git push origin azure_dev_spaces
}

function clean_up {
        rm -rf remote-monitoring-services-dotnet
        rm -rf device-simulation-dotnet
}

function start_all_services_nohup {
         for msfolder in ${!microservices[@]}; do
                  location=${microservicefolders[${msfolder}]}
                  cd "$location/WebService"
                  nohup azds up -v > ../../../"$location.azds.op" 2>&1 &
         done
}

function main {
        clean_up
        clone_repos
        cd remote-monitoring-services-dotnet
        checkout_and_merge_master
        start_all_services_nohup
        cd ..
        cd device-simulation-dotnet
        checkout_and_merge_master
        start_all_services_nohup
}

main
