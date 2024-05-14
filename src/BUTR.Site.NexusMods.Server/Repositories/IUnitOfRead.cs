using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

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

[TransientService<IUnitOfRead>]
internal class UnitOfRead : IUnitOfRead
{
    private readonly IServiceScope _serviceScope;
    private readonly AppDbContextRead _dbContext;

    public IAutocompleteEntityRepositoryRead Autocompletes { get; }

    public IQuartzExecutionLogEntityRepositoryRead QuartzExecutionLogs { get; }

    public IExceptionTypeRepositoryRead ExceptionTypes { get; }

    public ICrashReportEntityRepositoryRead CrashReports { get; }
    public ICrashReportToMetadataEntityRepositoryRead CrashReportToMetadatas { get; }
    public ICrashReportToModuleMetadataEntityRepositoryRead CrashReportModuleInfos { get; }
    public ICrashReportToFileIdEntityRepositoryRead CrashReportToFileIds { get; }
    public ICrashReportIgnoredFileEntityRepositoryRead CrashReportIgnoredFileIds { get; }

    public IStatisticsTopExceptionsTypeEntityRepositoryRead StatisticsTopExceptionsTypes { get; }
    public IStatisticsCrashScoreInvolvedEntityRepositoryRead StatisticsCrashScoreInvolveds { get; }

    public INexusModsArticleEntityRepositoryRead NexusModsArticles { get; }

    public INexusModsModToModuleEntityRepositoryRead NexusModsModModules { get; }
    public INexusModsModToNameEntityRepositoryRead NexusModsModName { get; }
    public INexusModsModToModuleInfoHistoryEntityRepositoryRead NexusModsModToModuleInfoHistory { get; }
    public INexusModsModToFileUpdateEntityRepositoryRead NexusModsModToFileUpdates { get; }

    public INexusModsUserRepositoryRead NexusModsUsers { get; }
    public INexusModsUserToNameEntityRepositoryRead NexusModsUserToName { get; }
    public INexusModsUserToCrashReportEntityRepositoryRead NexusModsUserToCrashReports { get; }
    public INexusModsUserToNexusModsModEntityRepositoryRead NexusModsUserToNexusModsMods { get; }
    public INexusModsUserToModuleEntityRepositoryRead NexusModsUserToModules { get; }

    public INexusModsUserToIntegrationGitHubEntityRepositoryRead NexusModsUserToGitHub { get; }
    public INexusModsUserToIntegrationDiscordEntityRepositoryRead NexusModsUserToDiscord { get; }
    public INexusModsUserToIntegrationGOGEntityRepositoryRead NexusModsUserToGOG { get; }
    public INexusModsUserToIntegrationSteamEntityRepositoryRead NexusModsUserToSteam { get; }

    public IIntegrationGitHubTokensEntityRepositoryRead IntegrationGitHubTokens { get; }
    public IIntegrationDiscordTokensEntityRepositoryRead IntegrationDiscordTokens { get; }
    public IIntegrationGOGTokensEntityRepositoryRead IntegrationGOGTokens { get; }
    public IIntegrationGOGToOwnedTenantEntityRepositoryRead IntegrationGOGToOwnedTenants { get; }
    public IIntegrationSteamTokensEntityRepositoryRead IntegrationSteamTokens { get; }
    public IIntegrationSteamToOwnedTenantEntityRepositoryRead IntegrationSteamToOwnedTenants { get; }

    public UnitOfRead(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
        var dbContextFactory = _serviceScope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContextRead>>();
        _dbContext = dbContextFactory.CreateDbContext();

        var dbContextProvider = ActivatorUtilities.CreateInstance<AppDbContextProvider>(_serviceScope.ServiceProvider);
        dbContextProvider.Set(_dbContext);

        Autocompletes = ActivatorUtilities.CreateInstance<AutocompleteEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        QuartzExecutionLogs = ActivatorUtilities.CreateInstance<QuartzExecutionLogEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        ExceptionTypes = ActivatorUtilities.CreateInstance<ExceptionTypeRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        CrashReports = ActivatorUtilities.CreateInstance<CrashReportEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        CrashReportToMetadatas = ActivatorUtilities.CreateInstance<CrashReportToMetadataEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        CrashReportModuleInfos = ActivatorUtilities.CreateInstance<CrashReportToModuleMetadataEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        CrashReportToFileIds = ActivatorUtilities.CreateInstance<CrashReportToFileIdEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        CrashReportIgnoredFileIds = ActivatorUtilities.CreateInstance<CrashReportIgnoredFileEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        StatisticsTopExceptionsTypes = ActivatorUtilities.CreateInstance<StatisticsTopExceptionsTypeEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        StatisticsCrashScoreInvolveds = ActivatorUtilities.CreateInstance<StatisticsCrashScoreInvolvedEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        NexusModsArticles = ActivatorUtilities.CreateInstance<NexusModsArticleEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        NexusModsModModules = ActivatorUtilities.CreateInstance<NexusModsModToModuleEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsModName = ActivatorUtilities.CreateInstance<NexusModsModToNameEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsModToModuleInfoHistory = ActivatorUtilities.CreateInstance<NexusModsModToModuleInfoHistoryEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsModToFileUpdates = ActivatorUtilities.CreateInstance<NexusModsModToFileUpdateEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        NexusModsUsers = ActivatorUtilities.CreateInstance<NexusModsUserRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsUserToName = ActivatorUtilities.CreateInstance<NexusModsUserToNameEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsUserToCrashReports = ActivatorUtilities.CreateInstance<NexusModsUserToCrashReportEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsUserToNexusModsMods = ActivatorUtilities.CreateInstance<NexusModsUserToNexusModsModEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsUserToModules = ActivatorUtilities.CreateInstance<NexusModsUserToModuleEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        NexusModsUserToGitHub = ActivatorUtilities.CreateInstance<NexusModsUserToIntegrationGitHubEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsUserToDiscord = ActivatorUtilities.CreateInstance<NexusModsUserToIntegrationDiscordEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsUserToGOG = ActivatorUtilities.CreateInstance<NexusModsUserToIntegrationGOGEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        NexusModsUserToSteam = ActivatorUtilities.CreateInstance<NexusModsUserToIntegrationSteamEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);

        IntegrationGitHubTokens = ActivatorUtilities.CreateInstance<IntegrationGitHubTokensEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        IntegrationDiscordTokens = ActivatorUtilities.CreateInstance<IntegrationDiscordTokensEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        IntegrationGOGTokens = ActivatorUtilities.CreateInstance<IntegrationGOGTokensEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        IntegrationGOGToOwnedTenants = ActivatorUtilities.CreateInstance<IntegrationGOGToOwnedTenantEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        IntegrationSteamTokens = ActivatorUtilities.CreateInstance<IntegrationSteamTokensEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
        IntegrationSteamToOwnedTenants = ActivatorUtilities.CreateInstance<IntegrationSteamToOwnedTenantEntityRepository>(_serviceScope.ServiceProvider, dbContextProvider);
    }

    public void Dispose()
    {
        _dbContext.ChangeTracker.Clear();

        _serviceScope.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _dbContext.ChangeTracker.Clear();

        if (_serviceScope is IAsyncDisposable serviceScopeAsyncDisposable)
            await serviceScopeAsyncDisposable.DisposeAsync();
        else
            _serviceScope.Dispose();
    }
}