using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Services;

public interface ISteamCommunityClient
{
    Task<bool> ConfirmIdentityAsync(Dictionary<string, string> parameters, CancellationToken ct);
}

public sealed class SteamCommunityClient : ISteamCommunityClient
{
    private readonly HttpClient _httpClient;

    public SteamCommunityClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<bool> ConfirmIdentityAsync(Dictionary<string, string> parameters, CancellationToken ct)
    {
        var query = new Dictionary<string, string>()
        {
            {"openid.ns", "http://specs.openid.net/auth/2.0"},
            {"openid.mode", "check_authentication"},
        };
        foreach (var parameter in parameters)
            query.TryAdd(parameter.Key, parameter.Value);

        using var request = new HttpRequestMessage(HttpMethod.Post, "openid/login")
        {
            Content = new FormUrlEncodedContent(query)
        };
        using var response = await _httpClient.SendAsync(request, ct);

        var responseString = await response.Content.ReadAsStringAsync(ct);

        return responseString.Contains("is_valid:true");
    }
}