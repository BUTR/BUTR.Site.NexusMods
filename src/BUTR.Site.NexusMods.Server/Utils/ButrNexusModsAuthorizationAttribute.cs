using BUTR.Authentication.NexusMods.Authentication;

using Microsoft.AspNetCore.Authorization;

namespace BUTR.Site.NexusMods.Server.Utils;

public class ButrNexusModsAuthorizationAttribute : AuthorizeAttribute
{
    public ButrNexusModsAuthorizationAttribute()
    {
        AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme;
    }
}