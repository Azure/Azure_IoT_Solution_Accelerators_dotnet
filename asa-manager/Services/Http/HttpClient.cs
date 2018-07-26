// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Http
{
    public interface IHttpClient
    {
        Task<IHttpResponse> GetAsync(IHttpRequest request);

        Task<IHttpResponse> PostAsync(IHttpRequest request);

        Task<IHttpResponse> PutAsync(IHttpRequest request);

        Task<IHttpResponse> PatchAsync(IHttpRequest request);

        Task<IHttpResponse> DeleteAsync(IHttpRequest request);

        Task<IHttpResponse> HeadAsync(IHttpRequest request);

        Task<IHttpResponse> OptionsAsync(IHttpRequest request);
        Task<T> GetJsonAsync<T>(string uri, string description, bool acceptNotFound = false);
    }

    public class HttpClient : IHttpClient
    {
        private readonly ILogger log;

        public HttpClient(ILogger logger)
        {
            this.log = logger;
        }

        public async Task<IHttpResponse> GetAsync(IHttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Get);
        }

        public async Task<IHttpResponse> PostAsync(IHttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Post);
        }

        public async Task<IHttpResponse> PutAsync(IHttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Put);
        }

        public async Task<IHttpResponse> PatchAsync(IHttpRequest request)
        {
            return await this.SendAsync(request, new HttpMethod("PATCH"));
        }

        public async Task<IHttpResponse> DeleteAsync(IHttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Delete);
        }

        public async Task<IHttpResponse> HeadAsync(IHttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Head);
        }

        public async Task<IHttpResponse> OptionsAsync(IHttpRequest request)
        {
            return await this.SendAsync(request, HttpMethod.Options);
        }

        public async Task<T> GetJsonAsync<T>(
            string uri,
            string description,
            bool acceptNotFound = false)
        {
            var request = new HttpRequest();
            request.SetUriFromString(uri);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("User-Agent", "ASA Manager");

            IHttpResponse response;

            try
            {
                response = await this.GetAsync(request);
            }
            catch (Exception e)
            {
                // TODO: Add retry for known HHTP errors
                this.log.Error("Request failed", () => new { uri, e });
                throw new ExternalDependencyException($"Failed to load {description}");
            }

            if (response.StatusCode == HttpStatusCode.NotFound && acceptNotFound)
            {
                return default(T);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                this.log.Error("Request failed", () => new { uri, response.StatusCode, response.Content });
                throw new ExternalDependencyException($"Unable to load {description}");
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception e)
            {
                this.log.Error($"Could not parse result from {uri}: {e.Message}", () => { });
                throw new ExternalDependencyException($"Could not parse result from {uri}");
            }
        }

        private async Task<IHttpResponse> SendAsync(IHttpRequest request, HttpMethod httpMethod)
        {
            var clientHandler = new HttpClientHandler();
            using (var client = new System.Net.Http.HttpClient(clientHandler))
            {
                var httpRequest = new HttpRequestMessage
                {
                    Method = httpMethod,
                    RequestUri = request.Uri
                };

                SetServerSSLSecurity(request, clientHandler);
                SetTimeout(request, client);
                SetContent(request, httpMethod, httpRequest);
                SetHeaders(request, httpRequest);

                this.log.Debug("Sending request", () => new { httpMethod, request.Uri, request.Options });
                var now = DateTimeOffset.UtcNow;

                try
                {
                    using (var response = await client.SendAsync(httpRequest))
                    {
                        if (request.Options.EnsureSuccess) response.EnsureSuccessStatusCode();

                        return new HttpResponse
                        {
                            StatusCode = response.StatusCode,
                            Headers = response.Headers,
                            Content = await response.Content.ReadAsStringAsync()
                        };
                    }
                }
                catch (HttpRequestException e)
                {
                    var timeSpent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - now.ToUnixTimeMilliseconds();
                    var errorMessage = e.Message;
                    if (e.InnerException != null)
                    {
                        errorMessage += " - " + e.InnerException.Message;
                    }

                    this.log.Error("Request failed", () => new { timeSpent, httpMethod.Method, request.Uri, errorMessage, e });

                    return new HttpResponse
                    {
                        StatusCode = 0,
                        Content = errorMessage
                    };
                }
                catch (TaskCanceledException e)
                {
                    var timeSpent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - now.ToUnixTimeMilliseconds();
                    this.log.Error("Request failed", () => new { timeSpent, httpMethod.Method, request.Uri, Message = e.Message + " The request timed out, the endpoint might be unreachable.", e });

                    return new HttpResponse
                    {
                        StatusCode = 0,
                        Content = e.Message + " The endpoint might be unreachable."
                    };
                }
                catch (Exception e)
                {
                    var timeSpent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - now.ToUnixTimeMilliseconds();
                    this.log.Error("Request failed", () => new { timeSpent, httpMethod.Method, request.Uri, e.Message, e });

                    return new HttpResponse
                    {
                        StatusCode = 0,
                        Content = e.Message
                    };
                }
            }
        }

        private static void SetContent(IHttpRequest request, HttpMethod httpMethod, HttpRequestMessage httpRequest)
        {
            if (httpMethod != HttpMethod.Post && httpMethod != HttpMethod.Put) return;

            httpRequest.Content = request.Content;
            if (request.ContentType != null && request.Content != null)
            {
                httpRequest.Content.Headers.ContentType = request.ContentType;
            }
        }

        private static void SetHeaders(IHttpRequest request, HttpRequestMessage httpRequest)
        {
            foreach (var header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }
        }

        private static void SetServerSSLSecurity(IHttpRequest request, HttpClientHandler clientHandler)
        {
            if (request.Options.AllowInsecureSSLServer)
            {
                clientHandler.ServerCertificateCustomValidationCallback = delegate { return true; };
            }
        }

        private static void SetTimeout(
            IHttpRequest request,
            System.Net.Http.HttpClient client)
        {
            client.Timeout = TimeSpan.FromMilliseconds(request.Options.Timeout);
        }
    }
}
