// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using static Microsoft.Azure.IoTSolutions.UIConfig.Services.Models.DeviceStatusQueries;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.Models
{
    public class FirmwareStatusQueries
    {
        public static IDictionary<QueryType, string> Queries = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, @"SELECT deviceId from devices where configurations.[[{0}]].status   
                  = 'Applied'"},
            { QueryType.SUCCESSFUL, @"SELECT deviceId FROM devices WHERE  
                 configurations.[[{0}]].status = 'Applied'  
                 AND properties.reported.firmware.fwUpdateStatus='Current'  
                 AND properties.reported.firmware.type='IoTDevKit'"},
            { QueryType.FAILED, @"SELECT deviceId FROM devices WHERE 
                 configurations.[[{0}]].status = 'Applied' 
                 AND properties.reported.firmware.fwUpdateStatus='Error'  
                 AND properties.reported.firmware.type='IoTDevKit'"}
        };
    }

    public class EdgeDeviceStatusQueries
    {
        public static IDictionary<QueryType, string> Queries = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, @"SELECT deviceId from devices.modules WHERE 
                moduleId = '$edgeAgent' 
                AND configurations.[[{0}]].status = 'Applied'" },
            { QueryType.SUCCESSFUL, @"SELECT deviceId from devices.modules WHERE 
                moduleId = '$edgeAgent' 
                AND configurations.[[{0}]].status = 'Applied' 
                AND properties.desired.$version = properties.reported.lastDesiredVersion  
                AND properties.reported.lastDesiredStatus.code = 200" },
            { QueryType.FAILED, @"SELECT deviceId FROM devices.modules WHERE 
                moduleId = '$edgeAgent' 
                AND configurations.[[{0}]].status = 'Applied' 
                AND properties.desired.$version = properties.reported.lastDesiredVersion 
                AND properties.reported.lastDesiredStatus.code != 200" }
        };
    }

    public class DefaultDeviceStatusQueries
    {
        public static IDictionary<QueryType, string> Queries = new Dictionary<QueryType, string>()
        {
            { QueryType.APPLIED, @"SELECT deviceId from devices where 
                 configurations.[[{0}]].status = 'Applied'" },
            { QueryType.SUCCESSFUL, String.Empty },
            { QueryType.FAILED, String.Empty }
        };
    }

    public class DeviceStatusQueries {

        private static Dictionary<string, IDictionary<QueryType, string>> AdmQueryMapping =
            new Dictionary<string, IDictionary<QueryType, string>>()
        {
            { ConfigType.Firmware.ToString(),
                    FirmwareStatusQueries.Queries }
        };

        internal static IDictionary<QueryType, string> GetQueries(string deploymentType, string configType)
        {
            if (deploymentType.Equals(PackageType.EdgeManifest.ToString()))
            {
                return EdgeDeviceStatusQueries.Queries;
            }

            return AdmQueryMapping.TryGetValue(configType, 
                    out IDictionary<QueryType, string> value) ? value : DefaultDeviceStatusQueries.Queries;
        }

        public enum QueryType { APPLIED, SUCCESSFUL, FAILED };
    }
}
