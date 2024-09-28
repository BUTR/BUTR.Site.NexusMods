using System;

namespace BUTR.Site.NexusMods.Server.Repositories;

public interface IUnitOfRead : IDisposable, IAsyncDisposable
{
    IAutocompleteEntityRepositoryRead Autocompletes { get; }

    IQuartzExecutionLogEntityRepositoryRead QuartzExecutionLogs { get; }

    IExceptionTypeRepositoryRead ExceptionTypes { get; }

    ICrashReportEntityRepositoryRead CrashReports { get; }
    ICrashReportToMetadataEntityRepositoryRead CrashReportToMetadatas { get; }
    ICrashReportToModuleMetadataEntityRepositoryRead CrashReportModuleInfos { get; }
    ICrashReportToFileIdEntityRepositoryRead CrashReportToFileIds { get; }
    ICrashReportIgnoredFileEntityRepositoryRead CrashReportIgnoredFileIds { get; }

    IStatisticsTopExceptionsTypeEntityRepositoryRead StatisticsTopExceptionsTypes { get; }
    IStatisticsCrashScoreInvolvedEntityRepositoryRead StatisticsCrashScoreInvolveds { get; }
    IStatisticsCrashReportsPerDayEntityRepositoryRead StatisticsCrashReportsPerDay { get; }
    IStatisticsCrashReportsPerMonthEntityRepositoryRead StatisticsCrashReportsPerMonth { get; }

    INexusModsArticleEntityRepositoryRead NexusModsArticles { get; }

    INexusModsModToModuleEntityRepositoryRead NexusModsModModules { get; }
    INexusModsModToNameEntityRepositoryRead NexusModsModName { get; }

    INexusModsModToModuleInfoHistoryEntityRepositoryRead NexusModsModToModuleInfoHistory { get; }
    INexusModsModToFileUpdateEntityRepositoryRead NexusModsModToFileUpdates { get; }

    INexusModsUserRepositoryRead NexusModsUsers { get; }
    INexusModsUserToNameEntityRepositoryRead NexusModsUserToName { get; }
    INexusModsUserToCrashReportEntityRepositoryRead NexusModsUserToCrashReports { get; }
    INexusModsUserToNexusModsModEntityRepositoryRead NexusModsUserToNexusModsMods { get; }
    INexusModsUserToModuleEntityRepositoryRead NexusModsUserToModules { get; }


    INexusModsUserToIntegrationGitHubEntityRepositoryRead NexusModsUserToGitHub { get; }
    INexusModsUserToIntegrationDiscordEntityRepositoryRead NexusModsUserToDiscord { get; }
    INexusModsUserToIntegrationGOGEntityRepositoryRead NexusModsUserToGOG { get; }
    INexusModsUserToIntegrationSteamEntityRepositoryRead NexusModsUserToSteam { get; }

    IIntegrationGitHubTokensEntityRepositoryRead IntegrationGitHubTokens { get; }
    IIntegrationDiscordTokensEntityRepositoryRead IntegrationDiscordTokens { get; }
    IIntegrationGOGTokensEntityRepositoryRead IntegrationGOGTokens { get; }
    IIntegrationGOGToOwnedTenantEntityRepositoryRead IntegrationGOGToOwnedTenants { get; }
    IIntegrationSteamTokensEntityRepositoryRead IntegrationSteamTokens { get; }
    IIntegrationSteamToOwnedTenantEntityRepositoryRead IntegrationSteamToOwnedTenants { get; }
}