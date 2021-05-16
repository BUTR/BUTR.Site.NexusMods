using BUTR.CrashReportViewer.Shared.Models.NexusModsAPI;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Shared.Helpers
{
    public class NexusModsAPIClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NexusModsAPIClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<NexusModsValidateResponse?> ValidateAPIKey(string apiKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "v1/users/validate.json");
            request.Headers.Add("apikey", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("NexusModsAPI");
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            else
            {
                var responseType = await response.Content.ReadFromJsonAsync<NexusModsValidateResponse>();
                if (responseType is null || responseType.Key != apiKey)
                    return null;

                return responseType;
            }
        }

        public async Task<NexusModsModInfoResponse?> GetMod(string gameDomain, int modId, string apiKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/games/{gameDomain}/mods/{modId}.json");
            request.Headers.Add("apikey", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpClient = _httpClientFactory.CreateClient("NexusModsAPI");
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            else
            {
                return await response.Content.ReadFromJsonAsync<NexusModsModInfoResponse>();
            }
        }
    }
}