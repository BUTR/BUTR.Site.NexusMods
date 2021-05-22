using BUTR.CrashReportViewer.Server.Models.Contexts;

using Microsoft.EntityFrameworkCore;

namespace BUTR.CrashReportViewer.Server.Contexts
{
    public class Dummy1DbContext : DbContext
    {
        public Dummy1DbContext(DbContextOptions<Dummy1DbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ModTableEntityConfiguration());
        }
    }
    public class Dummy2DbContext : DbContext
    {
        public Dummy2DbContext(DbContextOptions<Dummy2DbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CrashReportTableEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserCrashReportTableEntityConfiguration());
        }
    }
    public class Dummy3DbContext : DbContext
    {
        public Dummy3DbContext(DbContextOptions<Dummy3DbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserCrashReportTableEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CrashReportTableEntityConfiguration());
        }
    }
    public class Dummy4DbContext : DbContext
    {
        public Dummy4DbContext(DbContextOptions<Dummy4DbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CacheTableEntityConfiguration());
        }
    }

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
            modelBuilder.ApplyConfiguration(new CacheTableEntityConfiguration());
        }
    }
}