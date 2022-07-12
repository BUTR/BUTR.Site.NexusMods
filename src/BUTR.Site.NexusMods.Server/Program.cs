using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server
{
    public static class Program
    {
        public static Task Main(string[] args) => CreateHostBuilder(args)
            .Build()
            .SeedDbContext<AppDbContext>()
            .RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}