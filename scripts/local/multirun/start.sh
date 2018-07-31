#!/bin/bash
APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../../ && pwd )"
source $APP_HOME/scripts/local/.env
cd $APP_HOME/scripts/local/multirun

sh check_dependencies.sh device-telemetry $azres
azres=$?
sh check_dependencies.sh iothub-manager $azres
azres=$(($azres+$?))
sh check_dependencies.sh pcs-auth $azres
azres=$(($azres+$?))
sh check_dependencies.sh pcs-config $azres
azres=$(($azres+$?))
sh check_dependencies.sh asa-manager $azres
azres=$(($azres+$?))
sh check_dependencies.sh pcs-storage-adapter $azres
azres=$(($azres+$?))

set -e

if [ $azres -ne 0 ]; then
	read  -n 1 -p "Have you created required Azure resources (Y/N)?" yn
	echo -e "\n"
   case $yn in
	   "Y") 
			echo -e "Please set the env variables in .env file.\n The file is located under scripts/local folder.";  
			exit 0
		;;
	   "N") 
			echo "Setting up Azure resources."; 
			$APP_HOME/scripts/local/multirun/create_azure_resources.sh;
		;;
		*)
			echo "Incorrect option. Please re-run the script."
			exit 0
		;;
   esac
fi

start $APP_HOME/remote-monitoring.sln

set +e
