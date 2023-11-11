using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Models.NexusModsAPI;
using BUTR.Site.NexusMods.Server.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class HttpContextExtensions
{
    public static ProfileModel GetProfile(this HttpContext context, NexusModsValidateResponse validate, ApplicationRole role, Dictionary<string, string> metadata)
    {
        var jsonSerializerOptions = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;

        return new ProfileModel
        {
            NexusModsUserId = validate.UserId,
            Name = validate.Name,
            Email = validate.Email,
            ProfileUrl = validate.ProfileUrl,
            IsPremium = validate.IsPremium,
            IsSupporter = validate.IsSupporter,
            Role = role,
            DiscordUserId = GetDiscordId(metadata, jsonSerializerOptions),
            GOGUserId = GetGOGId(metadata, jsonSerializerOptions),
            SteamUserId = GetSteamId(metadata, jsonSerializerOptions),
            HasTenantGame = context.OwnsTenantGame(context.GetTenant()),
            AvailableTenants = TenantId.Values.Select(x => new ProfileTenantModel
            {
                TenantId = x,
                Name = x.ToName(),
            }).ToImmutableArray(),
        };
    }

    public static ProfileModel GetProfile(this HttpContext context)
    {
        var jsonSerializerOptions = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;

        return new ProfileModel
        {
            NexusModsUserId = context.GetUserId(),
            Name = context.GetName(),
            Email = context.GetEMail(),
            ProfileUrl = context.GetProfileUrl(),
            IsPremium = context.GetIsPremium(),
            IsSupporter = context.GetIsSupporter(),
            Role = context.GetRole(),
            DiscordUserId = GetDiscordId(context.GetMetadata(), jsonSerializerOptions),
            GOGUserId = GetGOGId(context.GetMetadata(), jsonSerializerOptions),
            SteamUserId = GetSteamId(context.GetMetadata(), jsonSerializerOptions),
            HasTenantGame = context.OwnsTenantGame(context.GetTenant()),
            AvailableTenants = TenantId.Values.Select(x => new ProfileTenantModel
            {
                TenantId = x,
                Name = x.ToName(),
            }).ToImmutableArray(),
        };
    }

    public static NexusModsUserId GetUserId(this HttpContext context)
    {
        if (context.User.FindFirst(ButrNexusModsClaimTypes.UserId)?.Value is { } userId && uint.TryParse(userId, out var val))
            return NexusModsUserId.From(val);
        return NexusModsUserId.None;
    }

    public static TenantId GetTenant(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Tenant", out var values) && values.FirstOrDefault() is { } tenantStr && byte.TryParse(tenantStr, out var tenant))
            return TenantId.From(tenant);
        return TenantId.None;
    }

    public static NexusModsUserName GetName(this HttpContext context)
    {
        if (context.User.FindFirst(ButrNexusModsClaimTypes.Name)?.Value is { } name)
            return NexusModsUserName.From(name);
        return NexusModsUserName.Empty;
    }

    public static NexusModsUserEMail GetEMail(this HttpContext context)
    {
        if (context.User.FindFirst(ButrNexusModsClaimTypes.EMail)?.Value is { } email)
            return NexusModsUserEMail.From(email);
        return NexusModsUserEMail.Empty;
    }

    public static string GetProfileUrl(this HttpContext context) =>
        context.User.FindFirst(ButrNexusModsClaimTypes.ProfileUrl)?.Value ?? string.Empty;

    public static bool GetIsPremium(this HttpContext context) =>
        bool.TryParse(context.User.FindFirst(ButrNexusModsClaimTypes.IsPremium)?.Value ?? string.Empty, out var val) ? val : false;

    public static bool GetIsSupporter(this HttpContext context) =>
        bool.TryParse(context.User.FindFirst(ButrNexusModsClaimTypes.IsSupporter)?.Value ?? string.Empty, out var val) ? val : false;

    public static NexusModsApiKey GetAPIKey(this HttpContext context)
    {
        if (context.User.FindFirst(ButrNexusModsClaimTypes.APIKey)?.Value is { } apiKey)
            return NexusModsApiKey.From(apiKey);
        return NexusModsApiKey.None;
    }

    public static ApplicationRole GetRole(this HttpContext context)
    {
        if (context.User.FindFirst(ButrNexusModsClaimTypes.Role)?.Value is { } role)
            return ApplicationRole.From(role);
        return ApplicationRole.User;
    }

    public static bool OwnsTenantGame(this HttpContext context)
    {
        var tenant = context.GetTenant();
        var jsonSerializerOptions = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        return OwnsTenantGame(tenant, context.GetMetadata(), jsonSerializerOptions);
    }

    public static string? GetDiscordId(Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions) =>
        GetTypedMetadata(metadata, jsonSerializerOptions).Discord?.ExternalId;
    public static string? GetGOGId(Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions) =>
        GetTypedMetadata(metadata, jsonSerializerOptions).GOG?.ExternalId;
    public static string? GetSteamId(Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions) =>
        GetTypedMetadata(metadata, jsonSerializerOptions).Steam?.ExternalId;
    public static bool OwnsTenantGame(TenantId tenant, Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions) =>
        GetTypedMetadata(metadata, jsonSerializerOptions).OwnedTenants.Contains(tenant);

    public static ExternalDataHolder<DiscordOAuthTokens>? GetDiscordTokens(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        var typedMetadata = GetTypedMetadata(context.GetMetadata(), options);
        return typedMetadata.Discord;
    }

    public static ExternalDataHolder<GOGOAuthTokens>? GetGOGTokens(this HttpContext context)
    {
        var jsonSerializerOptions = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        var typedMetadata = GetTypedMetadata(context.GetMetadata(), jsonSerializerOptions);
        return typedMetadata.GOG;
    }

    public static ExternalDataHolder<Dictionary<string, string>>? GetSteamTokens(this HttpContext context)
    {
        var jsonSerializerOptions = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        var typedMetadata = GetTypedMetadata(context.GetMetadata(), jsonSerializerOptions);
        return typedMetadata.Steam;
    }

    public static bool OwnsTenantGame(this HttpContext context, TenantId tenant)
    {
        var jsonSerializerOptions = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        return GetTypedMetadata(context.GetMetadata(), jsonSerializerOptions).OwnedTenants.Contains(tenant);
    }

    public static UserTypedMetadata GetTypedMetadata(Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions)
    {
        if (!metadata.TryGetValue(nameof(UserTypedMetadata), out var typedMetadataRaw)) return new();
        return JsonSerializer.Deserialize<UserTypedMetadata>(typedMetadataRaw, jsonSerializerOptions) ?? new();
    }

    public static Dictionary<string, string> GetMetadata(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        if (context.User.FindFirst(ButrNexusModsClaimTypes.Metadata)?.Value is { } json)
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, options) ?? new();
        return new();
    }
}