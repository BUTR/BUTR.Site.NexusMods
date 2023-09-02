using BUTR.Site.NexusMods.Shared;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BUTR.Site.NexusMods.Server.Models.Database;

/// <summary>
/// Building block
/// </summary>
public sealed record NexusModsUserEntity : IEntity
{
    public required int NexusModsUserId { get; init; }
    public NexusModsUserToNameEntity? Name { get; init; }
    public ICollection<NexusModsUserToRoleEntity> ToRoles { get; init; } = new List<NexusModsUserToRoleEntity>();
    public ICollection<NexusModsUserToModuleEntity> ToModules { get; init; } = new List<NexusModsUserToModuleEntity>();
    public ICollection<NexusModsUserToNexusModsModEntity> ToNexusModsMods { get; init; } = new List<NexusModsUserToNexusModsModEntity>();
    public ICollection<NexusModsUserToCrashReportEntity> ToCrashReports { get; init; } = new List<NexusModsUserToCrashReportEntity>();
    public ICollection<NexusModsArticleEntity> ToArticles { get; init; } = new List<NexusModsArticleEntity>();
    public NexusModsUserToIntegrationDiscordEntity? ToDiscord { get; init; }
    public NexusModsUserToIntegrationSteamEntity? ToSteam { get; init; }
    public NexusModsUserToIntegrationGOGEntity? ToGOG { get; init; }

    public override int GetHashCode() => HashCode.Combine(NexusModsUserId, Name);


    private NexusModsUserEntity() { }
    [SetsRequiredMembers]
    private NexusModsUserEntity(int userId, string? name = null, Tenant? tenant = null, string? role = null) : this()
    {
        NexusModsUserId = userId;
        Name = name is not null ? new()
        {
            NexusModsUser = this,
            Name = name,
        } : null;
        if (tenant is not null && role is not null)
        {
            ToRoles.Add(new()
            {
                NexusModsUser = this,
                TenantId = tenant.Value,
                Role = role,
            });
        }
    }

    public static NexusModsUserEntity Create(int userId) => new(userId);
    public static NexusModsUserEntity CreateWithName(int userId, string name) => new(userId, name: name);
    public static NexusModsUserEntity CreateWithRole(int userId, Tenant tenant, string role) => new(userId, tenant: tenant, role: role);
}