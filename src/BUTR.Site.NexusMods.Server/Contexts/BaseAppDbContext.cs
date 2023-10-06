using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models.Database;
using BUTR.Site.NexusMods.Server.Options;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

using System;

namespace BUTR.Site.NexusMods.Server.Contexts;

/// <summary>
/// Use directly for migrations only
/// </summary>
public class BaseAppDbContext : DbContext
{
    private readonly IEntityConfigurationFactory _entityConfigurationFactory;
    private readonly ConnectionStringsOptions _options;

    public required bool IsReadOnly { get; set; }

    public required DbSet<TenantEntity> Tenants { get; set; }

    public required DbSet<AutocompleteEntity> Autocompletes { get; set; }

    public required DbSet<ExceptionTypeEntity> ExceptionTypes { get; set; }

    public required DbSet<ModuleEntity> Modules { get; set; }

    public required DbSet<CrashReportEntity> CrashReports { get; set; }
    public required DbSet<CrashReportToMetadataEntity> CrashReportToMetadatas { get; set; }
    public required DbSet<CrashReportToModuleMetadataEntity> CrashReportModuleInfos { get; set; }
    public required DbSet<CrashReportToFileIdEntity> CrashReportToFileIds { get; set; }
    public required DbSet<CrashReportIgnoredFileEntity> CrashReportIgnoredFileIds { get; set; }

    public required DbSet<NexusModsUserEntity> NexusModsUsers { get; set; }
    public required DbSet<NexusModsUserToNameEntity> NexusModsUserToName { get; set; }
    public required DbSet<NexusModsUserToCrashReportEntity> NexusModsUserToCrashReports { get; set; }
    public required DbSet<NexusModsUserToNexusModsModEntity> NexusModsUserToNexusModsMods { get; set; }
    public required DbSet<NexusModsUserToModuleEntity> NexusModsUserToModules { get; set; }

    public required DbSet<NexusModsUserToIntegrationDiscordEntity> NexusModsUserToDiscord { get; set; }
    public required DbSet<NexusModsUserToIntegrationGOGEntity> NexusModsUserToGOG { get; set; }
    public required DbSet<NexusModsUserToIntegrationSteamEntity> NexusModsUserToSteam { get; set; }

    public required DbSet<IntegrationDiscordTokensEntity> IntegrationDiscordTokens { get; set; }
    public required DbSet<IntegrationGOGTokensEntity> IntegrationGOGTokens { get; set; }
    public required DbSet<IntegrationGOGToOwnedTenantEntity> IntegrationGOGToOwnedTenants { get; set; }
    public required DbSet<IntegrationSteamTokensEntity> IntegrationSteamTokens { get; set; }
    public required DbSet<IntegrationSteamToOwnedTenantEntity> IntegrationSteamToOwnedTenants { get; set; }

    public required DbSet<NexusModsArticleEntity> NexusModsArticles { get; set; }

    public required DbSet<NexusModsModEntity> NexusModsMods { get; set; }
    public required DbSet<NexusModsModToNameEntity> NexusModsModName { get; set; }
    public required DbSet<NexusModsModToModuleEntity> NexusModsModModules { get; set; }
    public required DbSet<NexusModsModToFileUpdateEntity> NexusModsModToFileUpdates { get; set; }
    public required DbSet<NexusModsModToModuleInfoHistoryEntity> NexusModsModToModuleInfoHistory { get; set; }

    public required DbSet<StatisticsTopExceptionsTypeEntity> StatisticsTopExceptionsTypes { get; set; }
    public required DbSet<StatisticsCrashScoreInvolvedEntity> StatisticsCrashScoreInvolveds { get; set; }

    public required DbSet<QuartzExecutionLogEntity> QuartzExecutionLogs { get; set; }

    public BaseAppDbContext(IOptions<ConnectionStringsOptions> connectionStringOptions, DbContextOptions<BaseAppDbContext> options, IEntityConfigurationFactory entityConfigurationFactory) : base(options)
    {
        _entityConfigurationFactory = entityConfigurationFactory ?? throw new ArgumentNullException(nameof(entityConfigurationFactory));
        _options = connectionStringOptions.Value ?? throw new ArgumentNullException(nameof(connectionStringOptions));
    }

    protected BaseAppDbContext(IOptions<ConnectionStringsOptions> connectionStringOptions, DbContextOptions options, IEntityConfigurationFactory entityConfigurationFactory) : base(options)
    {
        _entityConfigurationFactory = entityConfigurationFactory ?? throw new ArgumentNullException(nameof(entityConfigurationFactory));
        _options = connectionStringOptions.Value ?? throw new ArgumentNullException(nameof(connectionStringOptions));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("hstore");

        _entityConfigurationFactory.ApplyConfigurationWithTenant<AutocompleteEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<AutocompleteEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportIgnoredFileEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportToFileIdEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportToMetadataEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportToModuleMetadataEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfigurationWithTenant<ExceptionTypeEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfiguration<IntegrationDiscordTokensEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<IntegrationGOGTokensEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<IntegrationGOGToOwnedTenantEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<IntegrationSteamTokensEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<IntegrationSteamToOwnedTenantEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfigurationWithTenant<ModuleEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsArticleEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsModEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsModToFileUpdateEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsModToModuleEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsModToNameEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsModToModuleInfoHistoryEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfiguration<NexusModsUserEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsUserToCrashReportEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<NexusModsUserToIntegrationDiscordEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<NexusModsUserToIntegrationGOGEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsUserToModuleEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<NexusModsUserToNameEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsUserToNexusModsModEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsUserToRoleEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<NexusModsUserToIntegrationSteamEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfiguration<QuartzExecutionLogEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfigurationWithTenant<StatisticsCrashScoreInvolvedEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<StatisticsTopExceptionsTypeEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfiguration<TenantEntity>(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();

            optionsBuilder
                .UseNpgsql(IsReadOnly && !string.IsNullOrEmpty(_options.Replica) ? _options.Replica : _options.Main, opt =>
                {
                    opt.EnableRetryOnFailure();
                    opt.MigrationsHistoryTable("ef_migrations_history", "ef");
                })
                .AddPrepareInterceptor()
                .EnableSensitiveDataLogging()
                /*.UseLoggerFactory(LoggerFactory.Create(b => b.AddFilter(level => level >= LogLevel.Information)))*/;
        }
    }
}