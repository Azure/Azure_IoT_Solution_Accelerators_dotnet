// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation
{
    public class LogicApp : INotificationImplementation
    {
        private readonly IHttpClient httpClient;
        private readonly IHttpRequest httpRequest;
        private readonly ILogger logger;

        private string endointURL;
        private string content;
        private List<string> emailList;
        private string ruleId;
        private string ruleDescription;
        private string solutionName;

        public LogicApp(string endPointUrl, string solutionName, IHttpRequest httpRequest, IHttpClient httpClient, ILogger logger)
        {
            this.endointURL = endPointUrl;
            this.solutionName = solutionName;
            this.httpClient = httpClient;
            this.httpRequest = httpRequest;
            this.logger = logger;
        }

        public void SetMessage(string message, string ruleId, string ruleDescription)
        {
            this.content = message;
            this.ruleId = ruleId;
            this.ruleDescription = ruleDescription;
        }

        public void SetReceiver(List<string> receivers)
        {
            this.emailList = receivers;
        }

        public async Task<HttpStatusCode> Execute()
        {
            this.httpRequest.SetUriFromString(this.endointURL);
            string content = this.GeneratePayLoad();
            this.httpRequest.SetContent(content, Encoding.UTF8, "application/json");
            this.httpRequest.AddHeader("content-type", "application/json");
            // Client library handles Exception.
            return (await this.httpClient.PostAsync(this.httpRequest)).StatusCode;
        }

        private string GenerateRuleDetailUrl()
        {
            return "https://" + this.solutionName + ".azurewebsites.net/maintenance/rule/" + this.ruleId;
        }

        private string GeneratePayLoad()
        {
            var emailContent = "Alarm fired for rule ID: " + this.ruleId + "  Rule Description: " +
                this.ruleDescription + " Custom Message: " + this.content + "Alarm Detail Page: " + this.GenerateRuleDetailUrl();
            return "{\"emailAddress\" : " + JArray.FromObject(this.emailList) + ",\"template\": \"" + emailContent + "\"}";
        }
    }
}
