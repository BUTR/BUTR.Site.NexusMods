using BUTR.CrashReportViewer.Shared.Models.Contexts;

using Microsoft.EntityFrameworkCore;

namespace BUTR.CrashReportViewer.Shared.Contexts
{
    public class CrashReportsDbContext : DbContext
    {
        public DbSet<CrashReportTable> CrashReports { get; set; } = default!;
        public DbSet<UserCrashReportTable> UserCrashReport { get; set; } = default!;

        public CrashReportsDbContext(DbContextOptions<CrashReportsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CrashReportTableEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserCrashReportTableEntityConfiguration());
        }
    }
}