using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class HttpContextExtensions
{
    public static ProfileModel GetProfile(this HttpContext context, string role) => new()
    {
        UserId = context.GetUserId(),
        Name = context.GetName(),
        Email = context.GetEMail(),
        ProfileUrl = context.GetProfileUrl(),
        IsPremium = context.GetIsPremium(),
        IsSupporter = context.GetIsSupporter(),
        Role = role,
        DiscordUserId = context.GetDiscordTokens()?.ExternalId,
        SteamUserId = context.GetSteamTokens()?.ExternalId,
        GOGUserId = context.GetGOGTokens()?.ExternalId,
        HasBannerlord = context.GetHasBannerlord()
    };

    public static int GetUserId(this HttpContext context) =>
        int.TryParse(context.User.FindFirst(ButrNexusModsClaimTypes.UserId)?.Value ?? string.Empty, out var val) ? val : -1;

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

    public static ExternalDataHolder<DiscordOAuthTokens>? GetDiscordTokens(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        return context.GetMetadata().TryGetValue(ExternalStorageConstants.Discord, out var json) ? JsonSerializer.Deserialize<ExternalDataHolder<DiscordOAuthTokens>>(json, options) : null;
    }

    public static ExternalDataHolder<Dictionary<string, string>>? GetSteamTokens(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        return context.GetMetadata().TryGetValue(ExternalStorageConstants.Steam, out var json) ? JsonSerializer.Deserialize<ExternalDataHolder<Dictionary<string, string>>>(json, options) : null;
    }

    public static ExternalDataHolder<GOGOAuthTokens>? GetGOGTokens(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        return context.GetMetadata().TryGetValue(ExternalStorageConstants.GOG, out var json) ? JsonSerializer.Deserialize<ExternalDataHolder<GOGOAuthTokens>>(json, options) : null;
    }

    public static bool GetHasBannerlord(this HttpContext context) =>
        context.GetMetadata().Any(x => x.Key == "MB2B");

    public static Dictionary<string, string> GetMetadata(this HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
        if (context.User.FindFirst(ButrNexusModsClaimTypes.Metadata)?.Value is { } json)
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, options) ?? new();
        return new();
    }
}