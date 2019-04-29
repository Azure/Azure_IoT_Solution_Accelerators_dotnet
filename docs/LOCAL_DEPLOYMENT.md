Starting Microservices on local environment
=====
### Steps to create Azure resources
#### New Users
1) Run the [start.cmd or start.sh](https://github.com/Azure/remote-monitoring-services-dotnet/blob/master/scripts/local/launch/) script  (depending on your OS) located under launch *(scripts/local/launch)* folder. **OR**
2) Clone [pcs-cli](https://github.com/azure/pcs-cli/) (a cli for RM), build it and run "*pcs -s local*" command to create cloud resource and set the required environment variables locally. 
![start_new](https://user-images.githubusercontent.com/39531904/46452369-514b4a80-c750-11e8-8fab-6b6351d98f2b.PNG)
**Please Note:**
1) *If you have cloned azure-iot-pcs-remote-monitoring-dotnet repository, the scripts folder is present under services submodule (folder).*
2) *The start script requires **Node.js** to execute, please install latest stable Node 8 (donot use Node 10) before using this script. Also, this script might require administartive privileges or sudo permission as it tries to install [pcs-cli](https://github.com/Azure/pcs-cli) a cli interface for remote-monitoring deployments.*
3) *After creating the required azure resources through the script, please stop all the instances of your IDE and restart them. (Otherwise, the environment variables will not be picked up by microservices luanched from IDE)*
&nbsp; 
4) *For OS X the above script(s) may not be able to set the environment variables persistently. While provisioning, the script(s) also download a file containing environment variables. Please use this file to set the environemnt variables.*

#### Existing Users 
For users who have already created the required azure resources, please do one of the following: 
1) Set the environment variables globally on your machine. 
2) **VS Code:** Set the environment variables in the launch configurations of the IDE i.e. launch.json
3) **Visual Studio:** Set the environment variables for WebService project of the microservices by adding it to Properties → Debug → Environment variables

**Please Note:**
1) *Although not recommended, environment variables can also be set in appsettings.ini file present under WebService folder for each of the microservices.*
2) *Build tasks depend upon env variables. If env variables are being set in IDE configurations OR in appsettings file, the tasks may not work. You will have to build services separately by settings env variables.*

### Walk through for importing new Solution into the IDE
##### VS Code 
The preconfigured launch & task configuration(s) for VS code are included in the *scripts / local / launch / idesettings* folder. These settings are useful for building individual OR all microservices. 

##### Steps to import launch settings
1) Import this repository OR the services submodule from the azure-iot-pcs-remote-monitoring-dotnet.
2) Click the Add Configuration present under debug menu. (This will create .vscode folder) 
![vs](https://user-images.githubusercontent.com/39531904/44294751-611ad800-a251-11e8-8a14-7fc7bc3c6aed.PNG)
3) Replace the auto-created launch.json & task.json files under .vscode folder with files under vscode folder located under scripts/local/launch/idesettings. 
4) This will list all the debug/build configuration. 

##### Visual Studio
1) If you have set the environment variables using the scripts, then you could use the Visual Studio to debug by starting multiple startup projects. Please follow the instructions [here](https://msdn.microsoft.com/en-us/library/ms165413.aspx) to set multiple startup projects.
2) If you haven't set the environment variables, then they could be set in following files.
    1. appsettings.ini under WebService
    2. launchSettings.json under Properties folder under WebService.
3) For multiple startup project settings, please set only WebService projects as startup projects.   

Structure of the microservices
===
Each microservice comprises of following projects/folders. 
1) scripts 
2) WebService  
3) Service  
4) WebService.Test  
5) Service.Test

Description: 
1) Scripts  
The scripts folder is organized as follows\
i. **docker** sub folder for building docker containers of the current microservice.\
ii. **root** folder contains scripts for building and running services natively.\
&nbsp; 
![script folder structure](https://user-images.githubusercontent.com/39531904/44290937-10df4e00-a230-11e8-9cd4-a9c0644e166b.PNG "Caption")\
The docker build scripts require environment variables to be set up before execution. The run scripts can run both natively built and dockerized microservice. The run script under docker folder can also be independently used to pull and run published docker images. One can modify the tag and the account to pull different version or privately built docker images.
&nbsp; 

2) WebService  
It contains code for REST endpoints of the microservice.
&nbsp;  

3) Service  
It contains business logic and code interfacing various SDKs. 
&nbsp;

4) WebService.Test  
It contains unit tests for the REST endpoints of the microservice. 
&nbsp; 

5) Service  
It contains unit tests for the business logic and code interfacing various SDKs.
&nbsp;  

6) Other Projects  
The microservice might contain other projects such as RecurringTaskAgent etc.
