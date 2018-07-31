#!/usr/bin/env bash
set -e
log_file="env"

function version_formatter { 
	echo "$@" | awk -F. '{ printf("%d%03d%03d%03d\n", $1,$2,$3,$4); }'; 
}

function node_is_installed {
	# set to 1 initially
	local return_=0
	# set to 0 if not found
	local version=`node -v`

	if [ $(version_formatter $version) -ge $(version_formatter "9.0.0") ]; then
	return_=1
	fi
	# return value
	echo $return_
}



function npm_package_is_installed {
	# set to 1 initially
	local return_=1
	# set to 0 if not found
	set +e
	check=$(npm list --depth 1 --global iot-solutions | grep empty)
	if [ "$check" == "" ]; then
		return_=0
	fi
	set -e
	# return value
	echo $return_
}

chck_node=$(node_is_installed)
if [ $chck_node -ne 0 ]; then
	echo "Please install node with version 8.11.3 or lesser."
	exit 1
fi


pckg_chk=$(npm_package_is_installed "iot-solutions")
if [ $pckg_chk -ne 0 ]; then 
	echo "Installing IoT Solution npm package"
	npm install -g iot-solutions
	if [ $? -ne 0 ]; then
		echo "Unable to install node package 'iot-solutions'."
		exit 1
	fi
fi

echo "Login to Azure Account."
#pcs login


echo "Creating resources"
#pcs -t remotemonitoring -s local  | tee -a "$log_file"

echo "Resources are created."

tail -n 18 env >> envvars

set +e

while IFS='' read -r line || [[ -n "$line" ]]; do
    echo "Setting env vars: $line"    	
    export $line
    echo "export $line" >> ../.env
done < envvars


rm -rf envvars
rm -rf env

