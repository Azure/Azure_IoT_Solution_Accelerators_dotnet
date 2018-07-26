// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Http;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Helpers
{
    public interface IHttpClientWrapper
    {
        Task PostAsync(string uri, string description, object content = null);
    }

    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly ILogger logger;
        private readonly IHttpClient client;

        public HttpClientWrapper(
            ILogger logger,
            IHttpClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task PostAsync(
            string uri,
            string description,
            object content = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString(uri);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("User-Agent", "Config");
            if (uri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (content != null)
            {
                request.SetContent(content);
            }

            IHttpResponse response;

            try
            {
                response = await this.client.PostAsync(request);
            }
            catch (Exception e)
            {
                this.logger.Error("Request failed", () => new { uri, e });
                throw new ExternalDependencyException($"Failed to post {description}");
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                this.logger.Error("Request failed", () => new { uri, response.StatusCode, response.Content });
                throw new ExternalDependencyException($"Unable to post {description}");
            }
        }
    }
}
