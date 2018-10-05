[![Build][build-badge]][build-url]
[![Issues][issues-badge]][issues-url]
[![Gitter][gitter-badge]][gitter-url]

PCS Authentication and Authorization Overview
=============================================

This service allows to manage the users authorized to access Azure IoT
Solutions. Users management can be done using any identity service
provider supporting OpenId Connect.

Dependencies
============

The service depends on:

* [Azure Active Directory][aad-url] used to store users and providing
  the certificates to validate JWT tokens signature. Any identity
  provider supporting OpenId Connect should work though.
* Configuration settings to define the trusted Issuer and expected
  Audience.

How to use the microservice
===========================

## Quickstart - Running the service with Docker

1. Create an instance of [Azure Active Directory][aad-url] or simply
   reuse the instance coming with your Azure subscription
1. [Register][aad-register-app] an application in AAD
1. Get the Application ID and Issuer URL and store them in the
   [service configuration](WebService/appsettings.ini).
1. [Install Docker][docker-install-url]
1. Start the Auth service using docker compose:
   ```
   cd scripts
   cd docker
   run
   ```
1. Use an HTTP client such as [Postman][postman-url], to exercise the
   RESTful API.

## Running the service with Visual Studio or VS Code

1. [Install .NET Core 2.x][dotnet-install]
1. Install any recent edition of Visual Studio (Windows/MacOS) or Visual
   Studio Code (Windows/MacOS/Linux).
   * If you already have Visual Studio installed, then ensure you have
   [.NET Core Tools for Visual Studio 2017][dotnetcore-tools-url]
   installed (Windows only).
   * If you already have VS Code installed, then ensure you have the [C# for Visual Studio Code (powered by OmniSharp)][omnisharp-url] extension installed.
1. Create an instance of [Azure Active Directory][aad-url] or simply
   reuse the instance coming with your Azure subscription
1. Open the solution in Visual Studio or VS Code.
1. Define environment variables, as needed. See [Configuration and Environment variables](#configuration-and-environment-variables) for detailed information for setting these for your enviroment.
   1. `PCS_AUTH_AUDIENCE` = {your AAD application ID}
   1. `PCS_AUTH_ISSUER` = {your AAD issuer URL}
1. Start the WebService project (e.g. press F5).
1. Use an HTTP client such as [Postman][postman-url], to exercise the
   RESTful API.

## Project Structure

The solution contains the following projects and folders:

* **WebService**: ASP.NET Web API exposing a RESTful API for Authentication
  functionality, e.g. show the current user profile.
* **Services**: Library containing common business logic for interacting with
  Azure Active Directory.
* **WebService.Test**: Unit tests for the ASP.NET Web API project.
* **Services.Test**: Unit tests for the Services library.
* **scripts**: a folder containing scripts from the command line console,
  to build and run the solution, and other frequent tasks.

## Build and Run from the command line

The [scripts](scripts) folder contains scripts for many frequent tasks:

* `build`: compile all the projects and run the tests.
* `compile`: compile all the projects.
* `run`: compile the projects and run the service. This will prompt for
  elevated privileges in Windows to run the web service.

## Building a customized Docker image

The `scripts` folder includes a [docker](scripts/docker) subfolder with the
scripts required to package the service into a Docker image:

* `Dockerfile`: Docker image specifications
* `build`: build a Docker image and store the image in the local registry
* `run`: run the Docker container from the image stored in the local registry
* `content`: a folder with files copied into the image, including the entry
  point script

## Configuration and Environment variables

The service configuration is accessed via ASP.NET Core configuration
adapters, and stored in [appsettings.ini](WebService/appsettings.ini).
The INI format allows to store values in a readable format, with comments.

The configuration also supports references to environment variables, e.g. to
import credentials and network details. Environment variables are not
mandatory though, you can for example edit appsettings.ini and write
credentials directly in the file. Just be careful not sharing the changes,
e.g. sending a Pull Request or checking in the changes in git.

The configuration file in the repository references some environment
variables that need to be defined. Depending on the OS and the IDE used,
there are several ways to manage environment variables.

1. If you're using Visual Studio or Visual Studio for Mac, the environment
   variables are loaded from the project settings. Right click on WebService,
   and select Options/Properties, and find the section with the list of env
   vars. See [WebService/Properties/launchSettings.json](WebService/Properties/launchSettings.json).
1. Visual Studio Code loads the environment variables from
   [.vscode/launch.json](.vscode/launch.json)
1. When running the service **with Docker** or **from the command line**, the
   application will inherit environment variables values from the system. 
   * [This page][windows-envvars-howto-url] describes how to setup env vars
     in Windows. We suggest to edit and execute once the
     [env-vars-setup.cmd](scripts/env-vars-setup.cmd) script included in the
     repository. The settings will persist across terminal sessions and reboots.
   * For Linux and MacOS, we suggest to edit and execute
     [env-vars-setup](scripts/env-vars-setup) each time, before starting the
     service. Depending on OS and terminal, there are ways to persist values
     globally, for more information these pages should help:
     * https://stackoverflow.com/questions/13046624/how-to-permanently-export-a-variable-in-linux
     * https://stackoverflow.com/questions/135688/setting-environment-variables-in-os-x
     * https://help.ubuntu.com/community/EnvironmentVariables

Contributing to the solution
============================

Please follow our [contribution guidelines](CONTRIBUTING.md).  We love PRs too.

Troubleshooting
===============

{TODO}

Feedback
==========

Please enter issues, bugs, or suggestions as GitHub Issues here:
https://github.com/Azure/pcs-auth-dotnet/issues.





[build-badge]:https://solutionaccelerators.visualstudio.com/RemoteMonitoring/_apis/build/status/Consolidated%20Repo
[build-url]: https://solutionaccelerators.visualstudio.com/RemoteMonitoring/_build/latest?definitionId=22
[issues-badge]: https://img.shields.io/github/issues/azure/pcs-auth-dotnet.svg
[issues-url]: https://github.com/Azure/remote-monitoring-services-dotnet/issues?q=is%3Aissue+is%3Aopen+label%3Aauth
[gitter-badge]: https://img.shields.io/gitter/room/azure/iot-solutions.js.svg
[gitter-url]: https://gitter.im/azure/iot-solutions

[aad-url]: https://azure.microsoft.com/services/active-directory
[aad-register-app]: https://docs.microsoft.com/azure/active-directory/develop/active-directory-integrating-applications
[docker-install-url]: https://docs.docker.com/engine/installation/
[postman-url]: https://www.getpostman.com
[dotnet-install]: https://www.microsoft.com/net/learn/get-started
[vs-install-url]: https://www.visualstudio.com/downloads
[dotnetcore-tools-url]: https://www.microsoft.com/net/core#windowsvs2017
[omnisharp-url]: https://github.com/OmniSharp/omnisharp-vscode
[windows-envvars-howto-url]: https://superuser.com/questions/949560/how-do-i-set-system-environment-variables-in-windows-10
