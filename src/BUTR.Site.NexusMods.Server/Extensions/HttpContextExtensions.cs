using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Site.NexusMods.Server.Models.API;
using BUTR.Site.NexusMods.Shared.Helpers;

using Microsoft.AspNetCore.Http;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class HttpContextExtensions
    {
        public static ProfileModel GetProfile(this HttpContext context, string role) => new(
            context.GetUserId(),
            context.GetName(),
            context.GetEMail(),
            context.GetProfileUrl(),
            context.GetIsPremium(),
            context.GetIsSupporter(),
            role);
        
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
    }
}