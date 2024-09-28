using BUTR.Site.NexusMods.Server.Models.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System.Reflection;

namespace BUTR.Site.NexusMods.Server.Contexts;

/// <summary>
/// Use directly for migrations only
/// </summary>
public class BaseAppDbContext : DbContext
{
    private readonly NpgsqlDataSourceProvider _dataSourceProvider;
    private readonly IEntityConfigurationFactory _entityConfigurationFactory;

    public virtual bool IsReadOnly => false;

    public DbSet<TenantEntity> Tenants { get; set; } = default!;

    public DbSet<AutocompleteEntity> Autocompletes { get; set; } = default!;

    public DbSet<ExceptionTypeEntity> ExceptionTypes { get; set; } = default!;

    public DbSet<ModuleEntity> Modules { get; set; } = default!;

    public DbSet<CrashReportEntity> CrashReports { get; set; } = default!;
    public DbSet<CrashReportToMetadataEntity> CrashReportToMetadatas { get; set; } = default!;
    public DbSet<CrashReportToModuleMetadataEntity> CrashReportModuleInfos { get; set; } = default!;
    public DbSet<CrashReportToFileIdEntity> CrashReportToFileIds { get; set; } = default!;
    public DbSet<CrashReportIgnoredFileEntity> CrashReportIgnoredFileIds { get; set; } = default!;

    public DbSet<NexusModsUserEntity> NexusModsUsers { get; set; } = default!;
    public DbSet<NexusModsUserToNameEntity> NexusModsUserToName { get; set; } = default!;
    public DbSet<NexusModsUserToCrashReportEntity> NexusModsUserToCrashReports { get; set; } = default!;
    public DbSet<NexusModsUserToNexusModsModEntity> NexusModsUserToNexusModsMods { get; set; } = default!;
    public DbSet<NexusModsUserToModuleEntity> NexusModsUserToModules { get; set; } = default!;

    public DbSet<NexusModsUserToIntegrationGitHubEntity> NexusModsUserToGitHub { get; set; } = default!;
    public DbSet<NexusModsUserToIntegrationDiscordEntity> NexusModsUserToDiscord { get; set; } = default!;
    public DbSet<NexusModsUserToIntegrationGOGEntity> NexusModsUserToGOG { get; set; } = default!;
    public DbSet<NexusModsUserToIntegrationSteamEntity> NexusModsUserToSteam { get; set; } = default!;

    public DbSet<IntegrationGitHubTokensEntity> IntegrationGitHubTokens { get; set; } = default!;
    public DbSet<IntegrationDiscordTokensEntity> IntegrationDiscordTokens { get; set; } = default!;
    public DbSet<IntegrationGOGTokensEntity> IntegrationGOGTokens { get; set; } = default!;
    public DbSet<IntegrationGOGToOwnedTenantEntity> IntegrationGOGToOwnedTenants { get; set; } = default!;
    public DbSet<IntegrationSteamTokensEntity> IntegrationSteamTokens { get; set; } = default!;
    public DbSet<IntegrationSteamToOwnedTenantEntity> IntegrationSteamToOwnedTenants { get; set; } = default!;

    public DbSet<NexusModsArticleEntity> NexusModsArticles { get; set; } = default!;

    public DbSet<NexusModsModEntity> NexusModsMods { get; set; } = default!;
    public DbSet<NexusModsModToNameEntity> NexusModsModName { get; set; } = default!;
    public DbSet<NexusModsModToModuleEntity> NexusModsModModules { get; set; } = default!;
    public DbSet<NexusModsModToFileUpdateEntity> NexusModsModToFileUpdates { get; set; } = default!;
    public DbSet<NexusModsModToModuleInfoHistoryEntity> NexusModsModToModuleInfoHistory { get; set; } = default!;

    public DbSet<StatisticsTopExceptionsTypeEntity> StatisticsTopExceptionsTypes { get; set; } = default!;
    public DbSet<StatisticsCrashScoreInvolvedEntity> StatisticsCrashScoreInvolveds { get; set; } = default!;
    public DbSet<StatisticsCrashReportsPerDayEntity> StatisticsCrashReportsPerDays { get; set; } = default!;
    public DbSet<StatisticsCrashReportsPerMonthEntity> StatisticsCrashReportsPerMonths { get; set; } = default!;

    public DbSet<QuartzExecutionLogEntity> QuartzExecutionLogs { get; set; } = default!;

    public BaseAppDbContext(
        NpgsqlDataSourceProvider dataSourceProvider,
        IEntityConfigurationFactory entityConfigurationFactory,
        DbContextOptions<BaseAppDbContext> options) : base(options)
    {
        _dataSourceProvider = dataSourceProvider;
        _entityConfigurationFactory = entityConfigurationFactory;
    }

    protected BaseAppDbContext(
        NpgsqlDataSourceProvider dataSourceProvider,
        IEntityConfigurationFactory entityConfigurationFactory,
        DbContextOptions options) : base(options)
    {
        _dataSourceProvider = dataSourceProvider;
        _entityConfigurationFactory = entityConfigurationFactory;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("hstore");

        modelBuilder
            .HasDbFunction(typeof(Postgres.Functions).GetRuntimeMethod(nameof(Postgres.Functions.Log), [typeof(decimal)])!)
            .HasName("log");
        modelBuilder.HasDbFunction(typeof(Postgres.Functions).GetRuntimeMethod(nameof(Postgres.Functions.Log), [typeof(decimal), typeof(decimal)])!)
            .HasName("log");

        _entityConfigurationFactory.ApplyConfigurationWithTenant<AutocompleteEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<AutocompleteEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportIgnoredFileEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportToFileIdEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportToMetadataEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<CrashReportToModuleMetadataEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfigurationWithTenant<ExceptionTypeEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfiguration<IntegrationGitHubTokensEntity>(modelBuilder);
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
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsModToModuleInfoHistoryGameVersionEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfiguration<NexusModsUserEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfigurationWithTenant<NexusModsUserToCrashReportEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<NexusModsUserToIntegrationGitHubEntity>(modelBuilder);
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
        _entityConfigurationFactory.ApplyConfigurationWithTenant<StatisticsCrashReportsPerDateEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<StatisticsCrashReportsPerDayEntity>(modelBuilder);
        _entityConfigurationFactory.ApplyConfiguration<StatisticsCrashReportsPerMonthEntity>(modelBuilder);

        _entityConfigurationFactory.ApplyConfiguration<TenantEntity>(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dataSource = IsReadOnly ? _dataSourceProvider.ReadDataSource() : _dataSourceProvider.WriteDataSource();

        optionsBuilder
            .ConfigureWarnings(x => x.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning))
            .ConfigureWarnings(x => x.Ignore(RelationalEventId.ModelValidationKeyDefaultValueWarning))
            .ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>()
            .UseNpgsql(dataSource, opt =>
            {
                opt.MigrationsHistoryTable("ef_migrations_history", "ef");
            })
            .EnableSensitiveDataLogging()
            /*.UseLoggerFactory(LoggerFactory.Create(b => b.AddFilter(level => level >= LogLevel.Information)))*/;
    }
}