// Copyright (c) Microsoft. All rights reserved.

using System;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class DeviceJobServiceModel
    {
        public string DeviceId { get; }
        public DeviceJobStatus Status { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime EndTimeUtc { get; }
        public DateTime CreatedDateTimeUtc { get; }
        public DateTime LastUpdatedDateTimeUtc { get; }
        public MethodResultServiceModel Outcome { get; }
        public DeviceJobErrorServiceModel Error { get; }

        public DeviceJobServiceModel(DeviceJob deviceJob)
        {
            this.DeviceId = deviceJob.DeviceId;

            switch (deviceJob.Status)
            {
                case Azure.Devices.DeviceJobStatus.Pending:
                    this.Status = DeviceJobStatus.Pending;
                    break;

                case Azure.Devices.DeviceJobStatus.Scheduled:
                    this.Status = DeviceJobStatus.Scheduled;
                    break;

                case Azure.Devices.DeviceJobStatus.Running:
                    this.Status = DeviceJobStatus.Running;
                    break;

                case Azure.Devices.DeviceJobStatus.Completed:
                    this.Status = DeviceJobStatus.Completed;
                    break;

                case Azure.Devices.DeviceJobStatus.Failed:
                    this.Status = DeviceJobStatus.Failed;
                    break;

                case Azure.Devices.DeviceJobStatus.Canceled:
                    this.Status = DeviceJobStatus.Canceled;
                    break;
            }

            this.StartTimeUtc = deviceJob.StartTimeUtc;
            this.EndTimeUtc = deviceJob.EndTimeUtc;
            this.CreatedDateTimeUtc = deviceJob.CreatedDateTimeUtc;
            this.LastUpdatedDateTimeUtc = deviceJob.LastUpdatedDateTimeUtc;

            if (deviceJob.Outcome?.DeviceMethodResponse != null)
            {
                this.Outcome = new MethodResultServiceModel(deviceJob.Outcome.DeviceMethodResponse);
            }

            if (deviceJob.Error != null)
            {
                this.Error = new DeviceJobErrorServiceModel(deviceJob.Error);
            }
        }
    }

    /// <summary>
    /// refer to Microsoft.Azure.Devices.DeviceJobStatus
    /// </summary>
    public enum DeviceJobStatus
    {
        Pending = 0,
        Scheduled = 1,
        Running = 2,
        Completed = 3,
        Failed = 4,
        Canceled = 5
    }
}
