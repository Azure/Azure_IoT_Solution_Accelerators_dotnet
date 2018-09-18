:: Copyright (c) Microsoft. All rights reserved.
:: This script will install pcs-cli on this machine.
:: For more info on pcs-cli visit ().
@echo off

:: strlen of /scripts/local/launch/helpers/ => 30
SET APP_HOME=%~dp0
SET APP_HOME=%APP_HOME:~0,-30%

:main 
    call :check_dependencies
    if %errorlevel% == 1 (
        echo %errorlevel%
    )
    call :install_cli
    call :create_azure_resources
GOTO:EOF

call :main

::::::::::::::::::::: Functions ::::::::::::::::::::

:create_azure_resources
    :: Login to Azure Subscription
    echo Creating Azure resources ... This operation might fail if you are not logged in. Please login and try again.
    : Creating RM resources in Azure Subscription
    call pcs -t remotemonitoring -s local
GOTO:EOF

:install_cli
    cd ..\..\..\..\
    :: If pcs-cli is installed this step will do nothing
    git clone https://github.com/Azure/pcs-cli.git  > Nul 2>&1
    :: Build and Link CLI
    echo "Building pcs-cli ....."

    cd pcs-cli
    call npm install 
    call npm start
    call npm link > Nul 2>&1
    cd %APP_HOME%\scripts\local\launch
GOTO:EOF

:check_dependencies
    set result=0
    call  :node_is_installed result
    if %result% == -1 (
        echo "Please install node with version 8.11.3 or lesser."
        exit /b 1
    )
    call  :check_node_version result
    if %result% == 1 (
        echo "Please update your node with version 8.11.3 or lesser."
        exit /b 1
    )
GOTO:EOF

:copy_env
    xcopy ..\..\..\..\pcs-cli\.env .\
GOTO:EOF

::::::::::::::::::::: Helper Function ::::::::::::::::::::
:node_is_installed
    node -v > Nul 2>&1
    if "%errorlevel%" == "9009" (
        set `%~1=-1`
    ) else (
        set `%~1=1`
    )
GOTO:EOF

:check_node_version
    node -v >tmp.txt
    set /p NODE_VER=<tmp.txt
    call  :compare_installed_versions 8.11.3 %NODE_VER%
    if %errorlevel% == 1 set `%~1=1`
    if %errorlevel% == -1 set set `%~1=-1`

GOTO:EOF

:compare_installed_versions
    setlocal enableDelayedExpansion
    set "version1=%~1"
    set "version2=%~2"
    :LOOP
        :: Parse a number by breaking down using delimiters
        call :parse_numbers "%version1%" number1 version1
        call :parse_numbers "%version2%" number2 version2

        :: Check first number is greater or lesser than
        if %number1% gtr %number2% exit /b 1
        if %number1% lss %number2% exit /b -1
        
        :: Check if versions are not present
        if not defined version1 exit /b 0
        if not defined version2 exit /b 0
    GOTO:LOOP
GOTO:EOF

:parse_numbers  version  number  remainingNumber
:: Use . as the only delimiter
for /f "tokens=1* delims=." %%A in ("%~1") do (
  set "%~2=%%A"
  set "%~3=%%B"
)
exit /b
