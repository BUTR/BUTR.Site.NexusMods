using BUTR.Site.NexusMods.DependencyInjection;
using BUTR.Site.NexusMods.Server.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Repositories;

[TransientService<IUnitOfWrite>]
internal class UnitOfWrite : IUnitOfWrite
{
    private readonly AppDbContextWrite _dbContext;

    private IDbContextTransaction _dbContextTransaction;


    public IUpsertEntityFactory UpsertEntityFactory { get; private set; }

    public IAutocompleteEntityRepositoryWrite Autocompletes { get; }

    public IQuartzExecutionLogEntityRepositoryWrite QuartzExecutionLogs { get; }

    public IExceptionTypeRepositoryWrite ExceptionTypes { get; }

    public ICrashReportEntityRepositoryWrite CrashReports { get; }
    public ICrashReportToMetadataEntityRepositoryWrite CrashReportToMetadatas { get; }
    public ICrashReportToModuleMetadataEntityRepositoryWrite CrashReportModuleInfos { get; }
    public ICrashReportToFileIdEntityRepositoryWrite CrashReportToFileIds { get; }
    public ICrashReportIgnoredFileEntityRepositoryWrite CrashReportIgnoredFileIds { get; }

    public IStatisticsTopExceptionsTypeEntityRepositoryWrite StatisticsTopExceptionsTypes { get; }
    public IStatisticsCrashScoreInvolvedEntityRepositoryWrite StatisticsCrashScoreInvolveds { get; }
    public IStatisticsCrashReportsPerDayEntityRepositoryWrite StatisticsCrashReportsPerDay { get; }
    public IStatisticsCrashReportsPerMonthEntityRepositoryWrite StatisticsCrashReportsPerMonth { get; }

    public INexusModsArticleEntityRepositoryWrite NexusModsArticles { get; }

    public INexusModsModToModuleEntityRepositoryWrite NexusModsModModules { get; }
    public INexusModsModToNameEntityRepositoryWrite NexusModsModName { get; }
    public INexusModsModToModuleInfoHistoryEntityRepositoryWrite NexusModsModToModuleInfoHistory { get; }
    public INexusModsModToFileUpdateEntityRepositoryWrite NexusModsModToFileUpdates { get; }

    public ISteamWorkshopModToModuleEntityRepositoryWrite SteamWorkshopModModules { get; }
    public ISteamWorkshopModToNameEntityRepositoryWrite SteamWorkshopModName { get; }

    public INexusModsUserRepositoryWrite NexusModsUsers { get; }
    public INexusModsUserToNameEntityRepositoryWrite NexusModsUserToName { get; }
    public INexusModsUserToCrashReportEntityRepositoryWrite NexusModsUserToCrashReports { get; }
    public INexusModsUserToNexusModsModEntityRepositoryWrite NexusModsUserToNexusModsMods { get; }
    public INexusModsUserToSteamWorkshopModEntityRepositoryWrite NexusModsUserToSteamWorkshopMods { get; }
    public INexusModsUserToModuleEntityRepositoryWrite NexusModsUserToModules { get; }

    public INexusModsUserToIntegrationGitHubEntityRepositoryWrite NexusModsUserToGitHub { get; }
    public INexusModsUserToIntegrationDiscordEntityRepositoryWrite NexusModsUserToDiscord { get; }
    public INexusModsUserToIntegrationGOGEntityRepositoryWrite NexusModsUserToGOG { get; }
    public INexusModsUserToIntegrationSteamEntityRepositoryWrite NexusModsUserToSteam { get; }

    public IIntegrationGitHubTokensEntityRepositoryWrite IntegrationGitHubTokens { get; }
    public IIntegrationDiscordTokensEntityRepositoryWrite IntegrationDiscordTokens { get; }
    public IIntegrationGOGTokensEntityRepositoryWrite IntegrationGOGTokens { get; }
    public IIntegrationGOGToOwnedTenantEntityRepositoryWrite IntegrationGOGToOwnedTenants { get; }
    public IIntegrationSteamTokensEntityRepositoryWrite IntegrationSteamTokens { get; }
    public IIntegrationSteamToOwnedTenantEntityRepositoryWrite IntegrationSteamToOwnedTenants { get; }

    public UnitOfWrite(IServiceProvider serviceProvider)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContextWrite>>();
        _dbContext = dbContextFactory.CreateDbContext();

        var dbContextProvider = (IAppDbContextProvider) ActivatorUtilities.CreateInstance<AppDbContextProvider>(serviceProvider);
        dbContextProvider.Set(_dbContext);

        _dbContextTransaction = _dbContext.Database.BeginTransaction();
        UpsertEntityFactory = _dbContext.GetEntityFactory();

        Autocompletes = ActivatorUtilities.CreateInstance<AutocompleteEntityRepository>(serviceProvider, dbContextProvider);

        QuartzExecutionLogs = ActivatorUtilities.CreateInstance<QuartzExecutionLogEntityRepository>(serviceProvider, dbContextProvider);

        ExceptionTypes = ActivatorUtilities.CreateInstance<ExceptionTypeRepository>(serviceProvider, dbContextProvider);

        CrashReports = ActivatorUtilities.CreateInstance<CrashReportEntityRepository>(serviceProvider, dbContextProvider);
        CrashReportToMetadatas = ActivatorUtilities.CreateInstance<CrashReportToMetadataEntityRepository>(serviceProvider, dbContextProvider);
        CrashReportModuleInfos = ActivatorUtilities.CreateInstance<CrashReportToModuleMetadataEntityRepository>(serviceProvider, dbContextProvider);
        CrashReportToFileIds = ActivatorUtilities.CreateInstance<CrashReportToFileIdEntityRepository>(serviceProvider, dbContextProvider);
        CrashReportIgnoredFileIds = ActivatorUtilities.CreateInstance<CrashReportIgnoredFileEntityRepository>(serviceProvider, dbContextProvider);

        StatisticsTopExceptionsTypes = ActivatorUtilities.CreateInstance<StatisticsTopExceptionsTypeEntityRepository>(serviceProvider, dbContextProvider);
        StatisticsCrashScoreInvolveds = ActivatorUtilities.CreateInstance<StatisticsCrashScoreInvolvedEntityRepository>(serviceProvider, dbContextProvider);
        StatisticsCrashReportsPerDay = ActivatorUtilities.CreateInstance<StatisticsCrashReportsPerDayEntityRepository>(serviceProvider, dbContextProvider);
        StatisticsCrashReportsPerMonth = ActivatorUtilities.CreateInstance<StatisticsCrashReportsPerMonthEntityRepository>(serviceProvider, dbContextProvider);

        NexusModsArticles = ActivatorUtilities.CreateInstance<NexusModsArticleEntityRepository>(serviceProvider, dbContextProvider);

        NexusModsModModules = ActivatorUtilities.CreateInstance<NexusModsModToModuleEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsModName = ActivatorUtilities.CreateInstance<NexusModsModToNameEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsModToModuleInfoHistory = ActivatorUtilities.CreateInstance<NexusModsModToModuleInfoHistoryEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsModToFileUpdates = ActivatorUtilities.CreateInstance<NexusModsModToFileUpdateEntityRepository>(serviceProvider, dbContextProvider);

        SteamWorkshopModModules = ActivatorUtilities.CreateInstance<SteamWorkshopModToModuleEntityRepository>(serviceProvider, dbContextProvider);
        SteamWorkshopModName = ActivatorUtilities.CreateInstance<SteamWorkshopModToNameEntityRepository>(serviceProvider, dbContextProvider);

        NexusModsUsers = ActivatorUtilities.CreateInstance<NexusModsUserRepository>(serviceProvider, dbContextProvider);
        NexusModsUserToName = ActivatorUtilities.CreateInstance<NexusModsUserToNameEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsUserToCrashReports = ActivatorUtilities.CreateInstance<NexusModsUserToCrashReportEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsUserToNexusModsMods = ActivatorUtilities.CreateInstance<NexusModsUserToNexusModsModEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsUserToSteamWorkshopMods = ActivatorUtilities.CreateInstance<NexusModsUserToSteamWorkshopModEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsUserToModules = ActivatorUtilities.CreateInstance<NexusModsUserToModuleEntityRepository>(serviceProvider, dbContextProvider);

        NexusModsUserToGitHub = ActivatorUtilities.CreateInstance<NexusModsUserToIntegrationGitHubEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsUserToDiscord = ActivatorUtilities.CreateInstance<NexusModsUserToIntegrationDiscordEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsUserToGOG = ActivatorUtilities.CreateInstance<NexusModsUserToIntegrationGOGEntityRepository>(serviceProvider, dbContextProvider);
        NexusModsUserToSteam = ActivatorUtilities.CreateInstance<NexusModsUserToIntegrationSteamEntityRepository>(serviceProvider, dbContextProvider);

        IntegrationGitHubTokens = ActivatorUtilities.CreateInstance<IntegrationGitHubTokensEntityRepository>(serviceProvider, dbContextProvider);
        IntegrationDiscordTokens = ActivatorUtilities.CreateInstance<IntegrationDiscordTokensEntityRepository>(serviceProvider, dbContextProvider);
        IntegrationGOGTokens = ActivatorUtilities.CreateInstance<IntegrationGOGTokensEntityRepository>(serviceProvider, dbContextProvider);
        IntegrationGOGToOwnedTenants = ActivatorUtilities.CreateInstance<IntegrationGOGToOwnedTenantEntityRepository>(serviceProvider, dbContextProvider);
        IntegrationSteamTokens = ActivatorUtilities.CreateInstance<IntegrationSteamTokensEntityRepository>(serviceProvider, dbContextProvider);
        IntegrationSteamToOwnedTenants = ActivatorUtilities.CreateInstance<IntegrationSteamToOwnedTenantEntityRepository>(serviceProvider, dbContextProvider);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await _dbContext.SaveAsync(ct);
            await _dbContextTransaction.CommitAsync(ct);
            _dbContextTransaction = _dbContext.Database.BeginTransaction();
            UpsertEntityFactory = _dbContext.GetEntityFactory();
        }
        catch (Exception e)
        {
            throw;
            //return Result.Fail($"Error message: {e.Message}, Inner Exception: {e.InnerException}, StackTrace: {e.StackTrace}");
        }
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }
}