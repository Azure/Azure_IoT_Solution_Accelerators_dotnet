// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Extensions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using DeviceJobStatus = Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models.DeviceJobStatus;
using JobStatus = Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models.JobStatus;
using JobType = Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models.JobType;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services
{
    public interface IJobs
    {
        Task<IEnumerable<JobServiceModel>> GetJobsAsync(
            JobType? jobType,
            JobStatus? jobStatus,
            int? pageSize,
            string queryFrom,
            string queryTo);

        Task<JobServiceModel> GetJobsAsync(
            string jobId,
            bool? includeDeviceDetails,
            DeviceJobStatus? deviceJobStatus);

        Task<JobServiceModel> ScheduleDeviceMethodAsync(
            string jobId,
            string queryCondition,
            MethodParameterServiceModel parameter,
            DateTimeOffset startTimeUtc,
            long maxExecutionTimeInSeconds);

        Task<JobServiceModel> ScheduleTwinUpdateAsync(
            string jobId,
            string queryCondition,
            DeviceTwinServiceModel twin,
            DateTimeOffset startTimeUtc,
            long maxExecutionTimeInSeconds);
    }

    public class Jobs : IJobs
    {
        private JobClient jobClient;
        private RegistryManager registryManager;
        private IDeviceProperties deviceProperties;

        private const string DEVICE_DETAILS_QUERY_FORMAT = "select * from devices.jobs where devices.jobs.jobId = '{0}'";
        private const string DEVICE_DETAILS_QUERYWITH_STATUS_FORMAT = "select * from devices.jobs where devices.jobs.jobId = '{0}' and devices.jobs.status = '{1}'";

        public Jobs(IServicesConfig config, IDeviceProperties deviceProperties)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            this.deviceProperties = deviceProperties;

            IoTHubConnectionHelper.CreateUsingHubConnectionString(
                config.IoTHubConnString,
                conn => { this.jobClient = JobClient.CreateFromConnectionString(conn); });

            IoTHubConnectionHelper.CreateUsingHubConnectionString(
                config.IoTHubConnString,
                conn => { this.registryManager = RegistryManager.CreateFromConnectionString(conn); });
        }

        public async Task<IEnumerable<JobServiceModel>> GetJobsAsync(
            JobType? jobType,
            JobStatus? jobStatus,
            int? pageSize,
            string queryFrom,
            string queryTo)
        {
            var from = DateTimeOffsetExtension.Parse(queryFrom, DateTimeOffset.MinValue);
            var to = DateTimeOffsetExtension.Parse(queryTo, DateTimeOffset.MaxValue);

            var query = this.jobClient.CreateQuery(
                JobServiceModel.ToJobTypeAzureModel(jobType),
                JobServiceModel.ToJobStatusAzureModel(jobStatus),
                pageSize);

            var results = new List<JobServiceModel>();
            while (query.HasMoreResults)
            {
                var jobs = await query.GetNextAsJobResponseAsync();
                results.AddRange(jobs
                    .Where(j => j.CreatedTimeUtc >= from && j.CreatedTimeUtc <= to)
                    .Select(r => new JobServiceModel(r)));
            }

            return results;
        }

        public async Task<JobServiceModel> GetJobsAsync(
            string jobId,
            bool? includeDeviceDetails,
            DeviceJobStatus? deviceJobStatus)
        {
            var result = await this.jobClient.GetJobAsync(jobId);

            if (!includeDeviceDetails.HasValue || !includeDeviceDetails.Value)
            {
                return new JobServiceModel(result);
            }

            // Device job query by status of 'Completed' or 'Cancelled' will fail with InternalServerError
            // https://github.com/Azure/azure-iot-sdk-csharp/issues/257
            var queryString = deviceJobStatus.HasValue ?
                string.Format(DEVICE_DETAILS_QUERYWITH_STATUS_FORMAT, jobId, deviceJobStatus.Value.ToString().ToLower()) :
                string.Format(DEVICE_DETAILS_QUERY_FORMAT, jobId);

            var query = this.registryManager.CreateQuery(queryString);

            var deviceJobs = new List<DeviceJob>();
            while (query.HasMoreResults)
            {
                deviceJobs.AddRange(await query.GetNextAsDeviceJobAsync());
            }

            return new JobServiceModel(result, deviceJobs);
        }

        public async Task<JobServiceModel> ScheduleTwinUpdateAsync(
            string jobId,
            string queryCondition,
            DeviceTwinServiceModel twin,
            DateTimeOffset startTimeUtc,
            long maxExecutionTimeInSeconds)
        {
            var result = await this.jobClient.ScheduleTwinUpdateAsync(
                jobId,
                queryCondition,
                twin.ToAzureModel(),
                startTimeUtc.DateTime,
                maxExecutionTimeInSeconds);

            // Update the deviceProperties cache, no need to wait
            var model = new DevicePropertyServiceModel();

            var tagRoot = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(twin.Tags)) as JToken;
            if (tagRoot != null)
            {
                model.Tags = new HashSet<string>(tagRoot.GetAllLeavesPath());
            }

            var reportedRoot = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(twin.ReportedProperties)) as JToken;
            if (reportedRoot != null)
            {
                model.Reported = new HashSet<string>(reportedRoot.GetAllLeavesPath());
            }
            var unused = deviceProperties.UpdateListAsync(model);

            return new JobServiceModel(result);
        }

        public async Task<JobServiceModel> ScheduleDeviceMethodAsync(
            string jobId,
            string queryCondition,
            MethodParameterServiceModel parameter,
            DateTimeOffset startTimeUtc,
            long maxExecutionTimeInSeconds)
        {
            var result = await this.jobClient.ScheduleDeviceMethodAsync(
                jobId, queryCondition,
                parameter.ToAzureModel(),
                startTimeUtc.DateTime,
                maxExecutionTimeInSeconds);
            return new JobServiceModel(result);
        }
    }
}
