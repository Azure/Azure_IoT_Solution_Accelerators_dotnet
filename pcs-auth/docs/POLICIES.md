Policies
========

Policies are used to determine allowed actions for specific users. For example,
by updating the *roles.json* for user roles, you can specify which actions are
allowed for each role.

Role Policy Example:

```json
{
    "Items": [
        {
            "Id": "a400a00b-f67c-42b7-ba9a-f73d8c67e433",
            "Role": "admin",
            "AllowedActions": [
                "DeleteAlarms",
                "UpdateAlarms",
                "CreateDevices",
                "DeleteDevices",
                "UpdateDevices",
                "CreateDeviceGroups",
                "DeleteDeviceGroups",
                "UpdateDeviceGroups",
                "CreateRules",
                "UpdateRules",
                "DeleteRules",
                "CreateJobs",
                "UpdateSimManagement"
            ]
        },
        {
            "Id": "e5bbd0f5-128e-4362-9dd1-8f253c6082d7",
            "Role": "readOnly",
            "AllowedActions": []
        }
    ]
}
```

| Field    | Type      | Example     | Description | 
|---------|-----------|-------------|-------------|
| Id      | string    | a400a00b-f67c-42b7-ba9a-f73d8c67e433 | The Id is the unique identifier that aligns with the Id in the AAD application manifest. |
| Role    | string    | Admin       | The name of the role type for the policy that aligns with the role specified in the AAD application manifest. |
| AllowedActions | string[] | `[ "DeleteAlarms,"UpdateAlarms" ]`| A list of action types that specified the management operations that a user is allowed to perform as part of the role policy. |

*Note:* The `Id` and `Role` must match the Id and Role in the AAD application manifest:

![image](https://user-images.githubusercontent.com/3317135/42849965-664520e0-89da-11e8-8900-398da4ce8c39.png)
