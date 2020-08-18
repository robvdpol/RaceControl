using Newtonsoft.Json;
using RaceControl.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RaceControl.Services
{
    public class RestClient : IRestClient
    {
        private const string MediaTypeJson = @"application/json";

        private readonly HttpClient _httpClient;

        public RestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TResponse> GetAsJsonAsync<TResponse>(string url, string userAgent = null)
        {
            HttpResponseMessage httpResponse;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                requestMessage.Headers.Accept.ParseAdd(MediaTypeJson);

                if (!string.IsNullOrWhiteSpace(userAgent))
                {
                    requestMessage.Headers.UserAgent.ParseAdd(userAgent);
                }

                httpResponse = await _httpClient.SendAsync(requestMessage);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(httpResponse.StatusCode.ToString());
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TResponse>(responseContent);

            return response;
        }

        public async Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest requestObject, IDictionary<string, string> requestHeaders = null, string token = null)
        {
            var requestJson = JsonConvert.SerializeObject(requestObject);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, MediaTypeJson);

            if (requestHeaders != null)
            {
                foreach (var requestHeader in requestHeaders)
                {
                    requestContent.Headers.Add(requestHeader.Key, requestHeader.Value);
                }
            }

            HttpResponseMessage httpResponse;

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
            {
                requestMessage.Content = requestContent;

                if (!string.IsNullOrWhiteSpace(token))
                {
                    requestMessage.Headers.Authorization = CreateJwtAuthorizationHeader(token);
                }

                httpResponse = await _httpClient.SendAsync(requestMessage);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(httpResponse.StatusCode.ToString());
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TResponse>(responseContent);

            return response;
        }

        private static AuthenticationHeaderValue CreateJwtAuthorizationHeader(string token)
        {
            return new AuthenticationHeaderValue("JWT", token);
        }
    }
}