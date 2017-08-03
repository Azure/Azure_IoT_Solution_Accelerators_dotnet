::  Prepare the environment variables used by the application.
::
::  For more information about finding IoT Hub settings, more information here:
::
::  * https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-create-through-portal#endpoints
::  * https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-getstarted
::

:: The port where Device Simulation web service is listening
SETX PCS_IOTHUBMANAGER_WEBSERVICE_PORT "9002"

:: see: Shared access policies => key name => Connection string
SETX PCS_IOTHUB_CONNSTRING "..."
