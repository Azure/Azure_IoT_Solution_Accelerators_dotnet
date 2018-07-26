using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class AlarmIdListApiModel
    {
        [JsonProperty(PropertyName = "Items")]
        public List<string> Items { get; set; }

        public AlarmIdListApiModel()
        {
            this.Items = null;
        }

        public AlarmIdListApiModel(List<string> items)
        {
            this.Items = items;
        }
    }
}
