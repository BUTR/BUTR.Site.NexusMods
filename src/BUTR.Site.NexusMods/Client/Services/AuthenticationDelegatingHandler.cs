using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly SimpleAuthenticationStateProvider _authenticationStateProvider;
        private readonly ITokenContainer _tokenContainer;

        public AuthenticationDelegatingHandler(SimpleAuthenticationStateProvider authenticationStateProvider, ITokenContainer tokenContainer)
        {
            _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            _tokenContainer = tokenContainer ?? throw new ArgumentNullException(nameof(tokenContainer));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (await _tokenContainer.GetTokenAsync(ct) is { } token)
                request.Headers.Authorization = new AuthenticationHeaderValue("BUTR-NexusMods", token);

            var response = await base.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _tokenContainer.SetTokenAsync(null, ct);
                await _tokenContainer.SetTokenTypeAsync(null, ct);
                _authenticationStateProvider.Notify();
            }

            return response;
        }
    }
}