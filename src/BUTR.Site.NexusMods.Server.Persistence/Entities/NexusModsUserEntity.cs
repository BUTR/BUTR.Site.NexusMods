using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public sealed record NexusModsUserEntity : IEntity
{
    public required NexusModsUserId NexusModsUserId { get; init; }
    public NexusModsUserToNameEntity? Name { get; init; }
    public ICollection<NexusModsUserToRoleEntity> ToRoles { get; init; } = new List<NexusModsUserToRoleEntity>();
    public ICollection<NexusModsUserToModuleEntity> ToModules { get; init; } = new List<NexusModsUserToModuleEntity>();
    public ICollection<NexusModsUserToNexusModsModEntity> ToNexusModsMods { get; init; } = new List<NexusModsUserToNexusModsModEntity>();
    public ICollection<NexusModsUserToSteamWorkshopModEntity> ToSteamWorkshopMods { get; init; } = new List<NexusModsUserToSteamWorkshopModEntity>();
    public ICollection<NexusModsUserToCrashReportEntity> ToCrashReports { get; init; } = new List<NexusModsUserToCrashReportEntity>();
    public ICollection<NexusModsArticleEntity> ToArticles { get; init; } = new List<NexusModsArticleEntity>();
    public NexusModsUserToIntegrationGitHubEntity? ToGitHub { get; init; }
    public NexusModsUserToIntegrationDiscordEntity? ToDiscord { get; init; }
    public NexusModsUserToIntegrationSteamEntity? ToSteam { get; init; }
    public NexusModsUserToIntegrationGOGEntity? ToGOG { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUserId, Name);


    private NexusModsUserEntity() { }
    [SetsRequiredMembers]
    private NexusModsUserEntity(NexusModsUserId userId, NexusModsUserName? name = null, TenantId? tenant = null, ApplicationRole? role = null) : this()
    {
        NexusModsUserId = userId;
        Name = name is { } nameVal ? new()
        {
            NexusModsUserId = userId,
            NexusModsUser = this,
            Name = nameVal,
        } : null;
        if (tenant is { } tenantVal && role is { } roleVal)
        {
            ToRoles.Add(new()
            {
                NexusModsUserId = userId,
                NexusModsUser = this,
                TenantId = tenantVal,
                Role = roleVal,
            });
        }
    }

    public static NexusModsUserEntity Create(NexusModsUserId userId) => new(userId);
    public static NexusModsUserEntity CreateWithName(NexusModsUserId userId, NexusModsUserName name) => new(userId, name: name);
    public static NexusModsUserEntity CreateWithRole(NexusModsUserId userId, TenantId tenant, ApplicationRole role) => new(userId, tenant: tenant, role: role);
}