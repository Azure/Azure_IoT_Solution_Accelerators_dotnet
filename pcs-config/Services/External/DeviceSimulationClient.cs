// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IDeviceSimulationClient
    {
        Task<SimulationApiModel> GetDefaultSimulationAsync();
        Task CreateDefaultSimulationAsync();
        Task UpdateSimulationAsync(SimulationApiModel model);
    }

    public class DeviceSimulationClient : IDeviceSimulationClient
    {
        private const int DEFAULT_SIMULATION_ID = 1;
        private readonly IHttpClientWrapper httpClient;
        private readonly string serviceUri;

        public DeviceSimulationClient(
            IHttpClientWrapper httpClient,
            IServicesConfig config)
        {
            this.httpClient = httpClient;
            this.serviceUri = config.DeviceSimulationApiUrl;
        }

        public async Task<SimulationApiModel> GetDefaultSimulationAsync()
        {
            return await this.httpClient.GetAsync<SimulationApiModel>($"{this.serviceUri}/simulations/{DEFAULT_SIMULATION_ID}", $"Simulation {DEFAULT_SIMULATION_ID}", true);
        }

        public async Task CreateDefaultSimulationAsync()
        {
            await this.httpClient.PostAsync($"{this.serviceUri}/simulations?template=default", "Default Simulation");
        }

        public async Task UpdateSimulationAsync(SimulationApiModel model)
        {
            await this.httpClient.PutAsync($"{this.serviceUri}/simulations/{model.Id}", $"Simulation {model.Id}", model);
        }
    }
}
