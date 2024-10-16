using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IUnitOfWrite : IDisposable, IAsyncDisposable
{
    IUpsertEntityFactory UpsertEntityFactory { get; }

    IAutocompleteEntityRepositoryWrite Autocompletes { get; }

    IQuartzExecutionLogEntityRepositoryWrite QuartzExecutionLogs { get; }

    IExceptionTypeRepositoryWrite ExceptionTypes { get; }

    ICrashReportEntityRepositoryWrite CrashReports { get; }
    ICrashReportToMetadataEntityRepositoryWrite CrashReportToMetadatas { get; }
    ICrashReportToModuleMetadataEntityRepositoryWrite CrashReportModuleInfos { get; }
    ICrashReportToFileIdEntityRepositoryWrite CrashReportToFileIds { get; }
    ICrashReportIgnoredFileEntityRepositoryWrite CrashReportIgnoredFileIds { get; }

    IStatisticsTopExceptionsTypeEntityRepositoryWrite StatisticsTopExceptionsTypes { get; }
    IStatisticsCrashScoreInvolvedEntityRepositoryWrite StatisticsCrashScoreInvolveds { get; }
    IStatisticsCrashReportsPerDayEntityRepositoryWrite StatisticsCrashReportsPerDay { get; }
    IStatisticsCrashReportsPerMonthEntityRepositoryWrite StatisticsCrashReportsPerMonth { get; }

    INexusModsArticleEntityRepositoryWrite NexusModsArticles { get; }

    INexusModsModToModuleEntityRepositoryWrite NexusModsModModules { get; }
    INexusModsModToNameEntityRepositoryWrite NexusModsModName { get; }
    INexusModsModToModuleInfoHistoryEntityRepositoryWrite NexusModsModToModuleInfoHistory { get; }
    INexusModsModToFileUpdateEntityRepositoryWrite NexusModsModToFileUpdates { get; }

    ISteamWorkshopModToModuleEntityRepositoryWrite SteamWorkshopModModules { get; }
    ISteamWorkshopModToNameEntityRepositoryWrite SteamWorkshopModName { get; }

    INexusModsUserRepositoryWrite NexusModsUsers { get; }
    INexusModsUserToNameEntityRepositoryWrite NexusModsUserToName { get; }
    INexusModsUserToCrashReportEntityRepositoryWrite NexusModsUserToCrashReports { get; }
    INexusModsUserToNexusModsModEntityRepositoryWrite NexusModsUserToNexusModsMods { get; }
    INexusModsUserToSteamWorkshopModEntityRepositoryWrite NexusModsUserToSteamWorkshopMods { get; }
    INexusModsUserToModuleEntityRepositoryWrite NexusModsUserToModules { get; }

    INexusModsUserToIntegrationGitHubEntityRepositoryWrite NexusModsUserToGitHub { get; }
    INexusModsUserToIntegrationDiscordEntityRepositoryWrite NexusModsUserToDiscord { get; }
    INexusModsUserToIntegrationGOGEntityRepositoryWrite NexusModsUserToGOG { get; }
    INexusModsUserToIntegrationSteamEntityRepositoryWrite NexusModsUserToSteam { get; }

    IIntegrationGitHubTokensEntityRepositoryWrite IntegrationGitHubTokens { get; }
    IIntegrationDiscordTokensEntityRepositoryWrite IntegrationDiscordTokens { get; }
    IIntegrationGOGTokensEntityRepositoryWrite IntegrationGOGTokens { get; }
    IIntegrationGOGToOwnedTenantEntityRepositoryWrite IntegrationGOGToOwnedTenants { get; }
    IIntegrationSteamTokensEntityRepositoryWrite IntegrationSteamTokens { get; }
    IIntegrationSteamToOwnedTenantEntityRepositoryWrite IntegrationSteamToOwnedTenants { get; }

    Task SaveChangesAsync(CancellationToken ct);
}