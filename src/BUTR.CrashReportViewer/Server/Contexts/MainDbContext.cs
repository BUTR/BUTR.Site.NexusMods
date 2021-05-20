using BUTR.CrashReportViewer.Server.Models.Contexts;

using Microsoft.EntityFrameworkCore;

namespace BUTR.CrashReportViewer.Server.Contexts
{
    public class MainDbContext : DbContext
    {
        public DbSet<ModTable> Mods { get; set; } = default!;
        public DbSet<CrashReportTable> CrashReports { get; set; } = default!;
        public DbSet<UserCrashReportTable> UserCrashReports { get; set; } = default!;

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ModTableEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CrashReportTableEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserCrashReportTableEntityConfiguration());
        }
    }
}