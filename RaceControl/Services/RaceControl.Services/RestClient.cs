using Newtonsoft.Json;
using RaceControl.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RaceControl.Services
{
    public class RestClient : IRestClient
    {
        private const string mediaTypeJson = @"application/json";

        protected readonly HttpClient _httpClient;

        public RestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TResponse> GetAsJsonAsync<TResponse>(string url)
        {
            var httpResponse = await _httpClient.GetAsync(url);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(httpResponse.StatusCode.ToString());
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TResponse>(responseContent);

            return response;
        }

        public async Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(string url, TRequest requestObject, IDictionary<string, string> requestHeaders = null)
        {
            var requestJson = JsonConvert.SerializeObject(requestObject);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, mediaTypeJson);

            if (requestHeaders != null)
            {
                foreach (var requestHeader in requestHeaders)
                {
                    requestContent.Headers.Add(requestHeader.Key, requestHeader.Value);
                }
            }

            var httpResponse = await _httpClient.PostAsync(url, requestContent);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(httpResponse.StatusCode.ToString());
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TResponse>(responseContent);

            return response;
        }
    }
}