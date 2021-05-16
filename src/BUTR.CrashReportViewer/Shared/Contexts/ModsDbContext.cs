using BUTR.CrashReportViewer.Shared.Models.Contexts;

using Microsoft.EntityFrameworkCore;

namespace BUTR.CrashReportViewer.Shared.Contexts
{
    public class ModsDbContext : DbContext
    {
        public DbSet<ModTable> Mods { get; set; } = default!;

        public ModsDbContext(DbContextOptions<ModsDbContext> options) : base(options)
        {
            var connection = Database.GetDbConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=WAL;";
            command.ExecuteNonQuery();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ModTableEntityConfiguration());
        }
    }
}