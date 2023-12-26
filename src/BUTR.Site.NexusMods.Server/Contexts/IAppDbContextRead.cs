using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;

namespace BUTR.Site.NexusMods.Server.Contexts;

public interface IAppDbContextRead
{
    DbSet<TenantEntity> Tenants { get; }

    DbSet<AutocompleteEntity> Autocompletes { get; }

    DbSet<ExceptionTypeEntity> ExceptionTypes { get; }

    DbSet<ModuleEntity> Modules { get; }

    DbSet<CrashReportEntity> CrashReports { get; }
    DbSet<CrashReportToMetadataEntity> CrashReportToMetadatas { get; }
    DbSet<CrashReportToModuleMetadataEntity> CrashReportModuleInfos { get; }
    DbSet<CrashReportToFileIdEntity> CrashReportToFileIds { get; }
    DbSet<CrashReportIgnoredFileEntity> CrashReportIgnoredFileIds { get; }

    DbSet<NexusModsUserEntity> NexusModsUsers { get; }
    DbSet<NexusModsUserToNameEntity> NexusModsUserToName { get; }
    DbSet<NexusModsUserToCrashReportEntity> NexusModsUserToCrashReports { get; }
    DbSet<NexusModsUserToNexusModsModEntity> NexusModsUserToNexusModsMods { get; }
    DbSet<NexusModsUserToModuleEntity> NexusModsUserToModules { get; }

    DbSet<NexusModsUserToIntegrationGitHubEntity> NexusModsUserToGitHub { get; }
    DbSet<NexusModsUserToIntegrationDiscordEntity> NexusModsUserToDiscord { get; }
    DbSet<NexusModsUserToIntegrationGOGEntity> NexusModsUserToGOG { get; }
    DbSet<NexusModsUserToIntegrationSteamEntity> NexusModsUserToSteam { get; }

    DbSet<IntegrationGitHubTokensEntity> IntegrationGitHubTokens { get; }
    DbSet<IntegrationDiscordTokensEntity> IntegrationDiscordTokens { get; }
    DbSet<IntegrationGOGTokensEntity> IntegrationGOGTokens { get; }
    DbSet<IntegrationGOGToOwnedTenantEntity> IntegrationGOGToOwnedTenants { get; }
    DbSet<IntegrationSteamTokensEntity> IntegrationSteamTokens { get; }
    DbSet<IntegrationSteamToOwnedTenantEntity> IntegrationSteamToOwnedTenants { get; }

    DbSet<NexusModsArticleEntity> NexusModsArticles { get; }

    DbSet<NexusModsModEntity> NexusModsMods { get; }
    DbSet<NexusModsModToNameEntity> NexusModsModName { get; }
    DbSet<NexusModsModToModuleEntity> NexusModsModModules { get; }
    DbSet<NexusModsModToFileUpdateEntity> NexusModsModToFileUpdates { get; }
    DbSet<NexusModsModToModuleInfoHistoryEntity> NexusModsModToModuleInfoHistory { get; }

    DbSet<StatisticsTopExceptionsTypeEntity> StatisticsTopExceptionsTypes { get; }
    DbSet<StatisticsCrashScoreInvolvedEntity> StatisticsCrashScoreInvolveds { get; }

    DbSet<QuartzExecutionLogEntity> QuartzExecutionLogs { get; }
}