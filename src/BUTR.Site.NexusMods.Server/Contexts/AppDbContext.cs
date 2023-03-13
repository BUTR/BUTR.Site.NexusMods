using BUTR.Site.NexusMods.Server.Contexts.Config;

using Microsoft.EntityFrameworkCore;

namespace BUTR.Site.NexusMods.Server.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("hstore");
            modelBuilder.HasDbFunction(typeof(Extensions.DbFunctionsExtensions).GetMethod(nameof(Extensions.DbFunctionsExtensions.HasKeyValue))!)
                .HasName("hstore_has_key_value");

            modelBuilder.ApplyConfiguration(new UserRoleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserMetadataEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserAllowedModsEntityConfiguration());
            modelBuilder.ApplyConfiguration(new NexusModsArticleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new NexusModsExposedModsEntityConfiguration());
            modelBuilder.ApplyConfiguration(new NexusModsFileUpdateEntityConfiguration());
            modelBuilder.ApplyConfiguration(new NexusModsModEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ModNexusModsManualLinkEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserCrashReportEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CrashReportEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CrashReportFileEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CrashReportIgnoredFilesEntityConfiguration());
            modelBuilder.ApplyConfiguration(new NexusModsUserToDiscordEntityConfiguration());
            modelBuilder.ApplyConfiguration(new DiscordLinkedRoleTokensEntityConfiguration());
        }
    }
}