using BUTR.Site.NexusMods.Server.Contexts.Config;

using Microsoft.EntityFrameworkCore;

namespace BUTR.Site.NexusMods.Server.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("hstore");
        modelBuilder.HasDbFunction(typeof(Extensions.DbFunctionsExtensions).GetMethod(nameof(Extensions.DbFunctionsExtensions.HasKeyValue))!)
            .HasName("hstore_has_key_value");
        modelBuilder.HasDbFunction(typeof(Extensions.DbFunctionsExtensions).GetMethod(nameof(Extensions.DbFunctionsExtensions.HasKeysValues))!)
            .HasName("hstore_has_keys_values");

        modelBuilder.ApplyConfiguration(new QuartzExecutionLogEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AutocompleteEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TopExceptionsTypeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserMetadataEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserAllowedModsEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NexusModsArticleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NexusModsExposedModsEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NexusModsFileUpdateEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NexusModsModEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ModNexusModsManualLinkEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NexusModsModManualLinkedNexusModsUsersEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserCrashReportEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CrashReportEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CrashReportFileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CrashReportIgnoredFilesEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CrashScoreInvolvedEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NexusModsUserToDiscordEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DiscordLinkedRoleTokensEntityConfiguration());
    }
}