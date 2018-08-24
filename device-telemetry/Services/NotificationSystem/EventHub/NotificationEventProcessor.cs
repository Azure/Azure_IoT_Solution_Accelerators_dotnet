// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public class NotificationEventProcessor : IEventProcessor
    {
        private readonly ILogger logger;
        private readonly IServicesConfig servicesConfig;
        private readonly INotification notification;

        public NotificationEventProcessor(
            ILogger logger,
            IServicesConfig servicesConfig,
            INotification notification)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
            this.notification = notification;
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            this.logger.Info($"Notification Event Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.", () => { });
            await context.CheckpointAsync();
        }

        public Task OpenAsync(PartitionContext context)
        {
            this.logger.Info($"Notification EventProcessor initialized. Partition: '{context.PartitionId}'", () => { });
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            this.logger.Error($"Error on Partition: {context.PartitionId}, Error: {error.Message}", () => { });
            return Task.CompletedTask;
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            /*
             Method implemented by the IEventProcessor interface.
             Eventdata is an array of bytes. Each eventdata when converted to UTF-8 string
             has more than one Json objects. So, this method uses GetAlertListFromString Helper
             method to get an Enumerable over the json objects in a single unit of EventData. 

            Checks if there is more than one action before executing the actions.
             */
            foreach (EventData eventData in messages)
            {
                var data = Encoding.UTF8.GetString(eventData.Body.Array);
                IEnumerable<object> alertObjects = GetAlertListFromString(data);
                foreach (object jsonObject in alertObjects)
                {
                    string temp = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                    AlarmNotificationAsaModel alarmNotification = JsonConvert.DeserializeObject<AlarmNotificationAsaModel>(temp);
                    this.notification.alarm = alarmNotification;
                    if (this.notification.alarm.Actions != null) await notification.execute();
                }
            }
            await context.CheckpointAsync();
        }

        // Modify to throw exceptions to the calling method. 
        public IEnumerable<object> GetAlertListFromString(string eventDataString)
        {
            /*
             * Input Format: { ..json1.. }{ ..json2.. }{ ..json3.. } ... { ..jsonN.. }
             * 
             * json string is a string that contains more than one Json object. 
             * Method returns an Enumerable over a list of Json objects.
             * Uses JsonTextReader and StringReader to read all the Json objects.
             * Uses { ..json1.. }{ ..json2.. }{ ..json3.. } ... { ..jsonN.. } format of input. 
             * INvalid string from the start of the string is removed and last of the string is ignored. 
             * 
             * Throws JsonReader Exception when invalid Json strings. 
             */
            IList<object> returnList = new List<object>();
            if ((eventDataString != null) && (eventDataString != ""))
            {
                // Confirming that the string doesn't get invalid characters. 
                eventDataString = eventDataString.Substring(eventDataString.IndexOf("{"));
                object temp = String.Empty;
                JsonSerializer serializer = new JsonSerializer();
                using (var stringReader = new StringReader(eventDataString))
                {
                    // JsonTextReader creates a stream of Json Objects with each field 
                    // being read as a Json Token.
                    // stream.Read() reads an entire Json object using the opening and cloing 
                    // braces to identify a unit json Object. 
                    using (var jsonReader = new JsonTextReader(stringReader))
                    {
                        jsonReader.SupportMultipleContent = true;
                        while (jsonReader.Read())
                        {
                            try
                            {
                                returnList.Add(serializer.Deserialize(jsonReader));
                            }
                            catch (JsonReaderException e)
                            {
                                this.logger.Info("Error parsing the json string", () => new { e });
                                break;
                            }
                            catch (Exception e)
                            {
                                this.logger.Info("Exception parsing the json string", () => new { e });
                                break;
                            }
                        }
                    }
                }
            }
            return returnList;
        }
    }
}
