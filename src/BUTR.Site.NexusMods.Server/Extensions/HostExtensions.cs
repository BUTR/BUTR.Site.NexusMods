using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class HostExtensions
    {
        public static IHost SeedDbContext<TDbContext>(this IHost host) where TDbContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TDbContext>>();
            try
            {
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed the database");
                throw;
            }

            return host;
        }
    }
}