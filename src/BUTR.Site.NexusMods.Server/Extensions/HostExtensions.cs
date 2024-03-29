using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IHost"/> objects.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Seeds the database context associated with the host.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <param name="host">The host whose database context to seed.</param>
    /// <returns>The same host for chaining.</returns>
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