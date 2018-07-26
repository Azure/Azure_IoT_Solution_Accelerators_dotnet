API specifications - Device Properties
======================================

## Get a list of device properties

The list of device properties.

Request:
```
GET /v1/deviceproperties
```

Response:
```
200 OK
Content-Type: application/json
```
```json
{
    "Items": [
        "tags.Purpose",
        "tags.IsSimulated",
        "tags.BatchId",
        "properties.reported.SupportedMethods",
        "properties.reported.Protocol",
        "properties.reported.FirmwareUpdateStatus",
        "properties.reported.DeviceMethodStatus"
    ],
    "$metadata": {
        "$type": "DevicePropertyList;1",
        "$url": "/v1/deviceproperties"
    }
}
```
