using Blazorise;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class AuthenticationAnd401DelegatingHandler : AuthenticationInjectionDelegatingHandler
    {
        public AuthenticationAnd401DelegatingHandler(ITokenContainer tokenContainer, INotificationService notificationService) : base(tokenContainer, notificationService) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var response = await base.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _tokenContainer.SetTokenAsync(null, ct);
                await _notificationService.Error("Authentication failure! Please log in again!", "Error!");
            }

            // Cloudflare timeout
            if ((int) response.StatusCode == 522)
            {
                Console.WriteLine(522);
                await _notificationService.Error("Backend is down! Notify about the issue on GitHub https://github.com/BUTR/BUTR.Site.NexusMods", "Error!");
            }

            return response;
        }
    }
}