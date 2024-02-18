using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace BUTR.Site.NexusMods.Server.Utils.BindingSources;

public class ClaimsValueProvider : BindingSourceValueProvider
{
    private readonly ClaimsPrincipal _principal;

    public ClaimsValueProvider(BindingSource bindingSource, ClaimsPrincipal principal) : base(bindingSource)
    {
        _principal = principal;
    }

    public override bool ContainsPrefix(string prefix) => _principal.HasClaim(claim => claim.Type == prefix);

    public override ValueProviderResult GetValue(string key)
    {
        var claims = _principal.FindAll(key).Select(claim => claim.Value).ToArray();
        return claims.Length != 0 ? new ValueProviderResult(claims, CultureInfo.InvariantCulture) : ValueProviderResult.None;
    }
}