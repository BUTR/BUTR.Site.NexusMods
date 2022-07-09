using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public class AuthenticationInjectionDelegatingHandler : DelegatingHandler
    {
        protected readonly ITokenContainer _tokenContainer;

        public AuthenticationInjectionDelegatingHandler(ITokenContainer tokenContainer)
        {
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (await _tokenContainer.GetTokenAsync(ct) is { } token && token.Type != "demo")
                request.Headers.Authorization = new AuthenticationHeaderValue("BUTR-NexusMods", token.Value);

            return await base.SendAsync(request, ct);
        }
    }
}