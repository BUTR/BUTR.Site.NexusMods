using Blazorise;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services;

public class AuthenticationInjectionDelegatingHandler : DelegatingHandler
{
    protected readonly ITokenContainer _tokenContainer;
    protected readonly INotificationService _notificationService;

    public AuthenticationInjectionDelegatingHandler(ITokenContainer tokenContainer, INotificationService notificationService)
    {
        _tokenContainer = tokenContainer;
        _notificationService = notificationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (await _tokenContainer.GetTokenAsync(ct) is { } token && token.Type != "demo")
            request.Headers.Authorization = new AuthenticationHeaderValue("BUTR-NexusMods", token.Value);

        try
        {
            return await base.SendAsync(request, ct);
        }
        catch (HttpRequestException e)
        {
            if (e.Message == "TypeError: Failed to fetch")
            {
                await _notificationService.Error("Backend is down! Notify about the issue on GitHub https://github.com/BUTR/BUTR.Site.NexusMods", "Error!");
            }

            throw;
        }
    }
}