using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client.Services
{
    public sealed class AuthenticationAnd401DelegatingHandler : AuthenticationInjectionDelegatingHandler
    {
        public AuthenticationAnd401DelegatingHandler(ITokenContainer tokenContainer) : base(tokenContainer) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var response = await base.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                await _tokenContainer.SetTokenAsync(null, ct);

            return response;
        }
    }
}