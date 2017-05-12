@ECHO off
setlocal enableextensions enabledelayedexpansion

:: strlen("\scripts\") => 9
SET APP_HOME=%~dp0
SET APP_HOME=%APP_HOME:~0,-9%
cd %APP_HOME%

WHERE /Q msbuild
IF %ERRORLEVEL% NEQ 0 (
  echo "msbuild" command not found.
  echo Make sure "msbuild" directory is in the PATH environment variable.
  GOTO FAIL
)

call .nuget\nuget restore
IF NOT ERRORLEVEL 0 GOTO FAIL
call msbuild /m /p:Configuration=Debug;Platform="Any CPU"
IF NOT ERRORLEVEL 0 GOTO FAIL
call msbuild /m /p:Configuration=Release;Platform="Any CPU"
IF NOT ERRORLEVEL 0 GOTO FAIL

:: - - - - - - - - - - - - - -
goto :END

:FAIL
echo Command failed
endlocal
exit /B 1

:END
endlocal
