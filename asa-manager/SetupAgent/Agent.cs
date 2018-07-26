// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Concurrency;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;

namespace SetupAgent
{
    public interface IAgent
    {
        Task RunAsync(CancellationToken runState);
    }

    public class Agent : IAgent
    {
        private readonly IAsaStorage messages;
        private readonly IAsaStorage alarms;
        private readonly ILogger log;
        private readonly IThreadWrapper thread;
        private CancellationToken runState;

        // In case of errors, retry every 5 seconds
        private const int PAUSE_BETWEEN_ATTEMPTS_MSEC = 5000;

        public Agent(
            IAsaStorage messages,
            IAsaStorage alarms,
            IServicesConfig config,
            IThreadWrapper thread,
            ILogger logger)
        {
            this.messages = messages;
            this.alarms = alarms;
            this.thread = thread;
            this.log = logger;

            this.messages.Initialize(config.MessagesStorageType, config.MessagesCosmosDbConfig);
            this.alarms.Initialize(config.AlarmsStorageType, config.AlarmsCosmosDbConfig);
        }

        public async Task RunAsync(CancellationToken runState)
        {
            this.runState = runState;

            this.log.Info("ASA Job Setup Agent running", () => { });

            await this.CreateMessagesTableAsync();
            await this.CreateAlarmsTableAsync();

            this.log.Info("Storage setup completed", () => { });
        }

        private async Task CreateMessagesTableAsync()
        {
            this.log.Info("Starting Messages storage loop", () => { });

            while (!this.runState.IsCancellationRequested)
            {
                try
                {
                    this.log.Info("Creating Messages storage", () => { });
                    await this.messages.SetupOutputStorageAsync();
                    this.log.Info("Messages storage created", () => { });
                    return;
                }
                catch (Exception e)
                {
                    this.log.Error("Messages storage setup failed. Unable to create messages table.", () => new { e });
                }

                this.thread.Sleep(PAUSE_BETWEEN_ATTEMPTS_MSEC);
            }

            this.log.Info("Leaving Messages storage loop", () => { });
        }

        private async Task CreateAlarmsTableAsync()
        {
            this.log.Info("Starting Alarms storage loop", () => { });

            while (!this.runState.IsCancellationRequested)
            {
                try
                {
                    this.log.Info("Creating Alarms storage", () => { });
                    await this.alarms.SetupOutputStorageAsync();
                    this.log.Info("Alarms storage created", () => { });
                    return;
                }
                catch (Exception e)
                {
                    this.log.Error("Alarms storage setup failed. Unable to create alarms table.", () => new { e });
                }

                this.thread.Sleep(PAUSE_BETWEEN_ATTEMPTS_MSEC);
            }

            this.log.Info("Leaving Alarms storage loop", () => { });
        }
    }
}
