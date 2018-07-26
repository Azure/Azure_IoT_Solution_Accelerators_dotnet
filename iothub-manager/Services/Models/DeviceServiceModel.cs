// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeviceServiceModel
    {
        public string Etag { get; set; }
        public string Id { get; set; }
        public int C2DMessageCount { get; set; }
        public DateTime LastActivity { get; set; }
        public bool Connected { get; set; }
        public bool Enabled { get; set; }
        public DateTime LastStatusUpdated { get; set; }
        public DeviceTwinServiceModel Twin { get; set; }
        public string IoTHubHostName { get; set; }
        public AuthenticationMechanismServiceModel Authentication { get; set; }

        public DeviceServiceModel(
            string etag,
            string id,
            int c2DMessageCount,
            DateTime lastActivity,
            bool connected,
            bool enabled,
            DateTime lastStatusUpdated,
            DeviceTwinServiceModel twin,
            AuthenticationMechanismServiceModel authentication,
            string ioTHubHostName)
        {
            this.Etag = etag;
            this.Id = id;
            this.C2DMessageCount = c2DMessageCount;
            this.LastActivity = lastActivity;
            this.Connected = connected;
            this.Enabled = enabled;
            this.LastStatusUpdated = lastStatusUpdated;
            this.Twin = twin;
            this.IoTHubHostName = ioTHubHostName;
            this.Authentication = authentication;
        }

        public DeviceServiceModel(Device azureDevice, DeviceTwinServiceModel twin, string ioTHubHostName) :
            this(
                etag: azureDevice.ETag,
                id: azureDevice.Id,
                c2DMessageCount: azureDevice.CloudToDeviceMessageCount,
                lastActivity: azureDevice.LastActivityTime,
                connected: azureDevice.ConnectionState.Equals(DeviceConnectionState.Connected),
                enabled: azureDevice.Status.Equals(DeviceStatus.Enabled),
                lastStatusUpdated: azureDevice.StatusUpdatedTime,
                twin: twin,
                ioTHubHostName: ioTHubHostName,
                authentication: new AuthenticationMechanismServiceModel(azureDevice.Authentication))
        {
        }

        public DeviceServiceModel(Device azureDevice, Twin azureTwin, string ioTHubHostName) :
            this(azureDevice, new DeviceTwinServiceModel(azureTwin), ioTHubHostName)
        {
        }

        public Device ToAzureModel(bool ignoreEtag = true)
        {
            var device = new Device(this.Id)
            {
                ETag = ignoreEtag ? null : this.Etag,
                Status = Enabled ? DeviceStatus.Enabled : DeviceStatus.Disabled,
                Authentication = this.Authentication == null ? null : this.Authentication.ToAzureModel()
            };

            return device;
        }
    }
}
