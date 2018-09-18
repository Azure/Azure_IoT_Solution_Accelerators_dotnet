#!/bin/bash
# Copyright (c) Microsoft. All rights reserved.

APP_HOME="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd ../../../../ && pwd )"

function version_formatter { 
	echo "$@" | awk -F. '{ printf("%d%03d%03d%03d\n", $1,$2,$3,$4); }'; 
}

function node_is_installed {
	# set to 1 initially
	local return_=0
	which node > /dev/null
	if [ $? -eq 0 ]; then
	       #"node is installed, skipping..."
	       return_=0
	else
	       return_=1   
	fi
	# return value
	echo $return_
}

function check_node_version {
	local return_=0	
	# set to 0 if not found
	local version=`node -v`
	if [ $(version_formatter $version) -ge $(version_formatter "9.0.0") ]; then
		return_=1
	fi
	# return value
	echo $return_
}

function check_dependencies {
	# check if node is installed
	local chck_node=$(node_is_installed)
	if [ $chck_node -ne 0 ]; then
		echo "Please install node with version 8.11.3 or lesser."
		exit 1
	fi
        
	local chck_node_v=$(check_node_version)
	if [ $chck_node_v -ne 0 ]; then
		echo "Please update your node with version 8.11.3 or lesser."
		exit 1
	fi
}

function install_cli {
	cd ../../../../
	# If pcs-cli is instlled this step will do nothing
	git clone https://github.com/Azure/pcs-cli.git 2> /dev/null
	# Build and Link CLI
	cd pcs-cli
	npm install && npm start && npm link 2> /dev/null
	cd $APP_HOME/scripts/local/launch
}

function create_resources {
	# Login to Azure Subscription
	echo "Creating resources ... This operation might fail if you are not logged in. Please login and try again."
	# Creating RM resources in Azure Subscription
	pcs -t remotemonitoring -s local
}

function copy_env {
	cp ../../../../pcs-cli/.env ./
}

function main {
	install_cli
	set -e
	check_dependencies
	create_resources
	#copy_env
	set +e
}

main

