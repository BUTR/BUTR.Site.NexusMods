using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class HttpContextExtensions
{
    public static ProfileModel GetProfile(this HttpContext context, string role, Dictionary<string, string> metadata)
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
            Role = role,
            DiscordUserId = GetDiscordId(metadata, jsonSerializerOptions),
            GOGUserId = GetGOGId(metadata, jsonSerializerOptions),
            SteamUserId = GetSteamId(metadata, jsonSerializerOptions),
            HasTenantGame = context.OwnsTenantGame(Tenant.Bannerlord),
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
            HasTenantGame = context.OwnsTenantGame(Tenant.Bannerlord),
        };
    }

    public static int GetUserId(this HttpContext context) =>
        int.TryParse(context.User.FindFirst(ButrNexusModsClaimTypes.UserId)?.Value ?? string.Empty, out var val) ? val : -1;

    public static Tenant? GetTenant(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Tenant", out var values) && values.FirstOrDefault() is { } tenantStr && Enum.TryParse(tenantStr, out Tenant tenant))
            return tenant;
        return null;
    }

    public static string GetName(this HttpContext context) =>
        context.User.FindFirst(ButrNexusModsClaimTypes.Name)?.Value ?? string.Empty;

    public static string GetEMail(this HttpContext context) =>
        context.User.FindFirst(ButrNexusModsClaimTypes.EMail)?.Value ?? string.Empty;

    public static string GetProfileUrl(this HttpContext context) =>
        context.User.FindFirst(ButrNexusModsClaimTypes.ProfileUrl)?.Value ?? string.Empty;

    public static bool GetIsPremium(this HttpContext context) =>
        bool.TryParse(context.User.FindFirst(ButrNexusModsClaimTypes.IsPremium)?.Value ?? string.Empty, out var val) ? val : false;

    public static bool GetIsSupporter(this HttpContext context) =>
        bool.TryParse(context.User.FindFirst(ButrNexusModsClaimTypes.IsSupporter)?.Value ?? string.Empty, out var val) ? val : false;

    public static string GetAPIKey(this HttpContext context) =>
        context.User.FindFirst(ButrNexusModsClaimTypes.APIKey)?.Value ?? string.Empty;

    public static string GetRole(this HttpContext context) =>
        context.User.FindFirst(ButrNexusModsClaimTypes.Role)?.Value ?? ApplicationRoles.User;

    public static bool OwnsTenantGame(this HttpContext context)
    {
        if (context.GetTenant() is not { } tenant) return false;

        var jsonSerializerOptions = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        return OwnsTenantGame(tenant, context.GetMetadata(), jsonSerializerOptions);
    }

    public static string? GetDiscordId(Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions) =>
        GetTypedMetadata(metadata, jsonSerializerOptions).Discord?.ExternalId;
    public static string? GetGOGId(Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions) =>
        GetTypedMetadata(metadata, jsonSerializerOptions).GOG?.ExternalId;
    public static string? GetSteamId(Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions) =>
        GetTypedMetadata(metadata, jsonSerializerOptions).Steam?.ExternalId;
    public static bool OwnsTenantGame(Tenant tenant, Dictionary<string, string> metadata, JsonSerializerOptions jsonSerializerOptions) =>
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

    public static bool OwnsTenantGame(this HttpContext context, Tenant tenant)
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