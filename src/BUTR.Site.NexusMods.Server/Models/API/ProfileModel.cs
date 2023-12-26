using System.Collections.Immutable;

namespace BUTR.Site.NexusMods.Server.Models.API;

public sealed record ProfileModel
{
    public required NexusModsUserId NexusModsUserId { get; init; }
    public required NexusModsUserName Name { get; init; }
    public required NexusModsUserEMail Email { get; init; }
    public required string ProfileUrl { get; init; }
    public required bool IsPremium { get; init; }
    public required bool IsSupporter { get; init; }
    public required ApplicationRole Role { get; init; }

    public required string? GitHubUserId { get; init; }
    public required string? DiscordUserId { get; init; }
    public required string? SteamUserId { get; init; }
    public required string? GOGUserId { get; init; }

    public required bool HasTenantGame { get; init; }

    public required ImmutableArray<ProfileTenantModel> AvailableTenants { get; init; }
}