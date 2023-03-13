using BUTR.Site.NexusMods.Server.Utils.Npgsql;

using Microsoft.EntityFrameworkCore;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder AddPrepareInterceptor(this DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.AddInterceptors(new PrepareCommandInterceptor());
    }
}