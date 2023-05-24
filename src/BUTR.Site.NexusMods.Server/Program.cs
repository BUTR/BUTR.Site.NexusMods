using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Quartz;

using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server;

public static class Program
{
    public static Task Main(string[] args) => CreateHostBuilder(args)
        .Build()
        .SeedDbContext<AppDbContext>()
        .RunAsync();

    public static IHostBuilder CreateHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services.AddQuartzHostedService(options =>
            {
                options.AwaitApplicationStarted = true;
                options.WaitForJobsToComplete = true;
            });
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            webBuilder.UseSentry();
        })
        .ConfigureLogging((ctx, builder) =>
        {
            builder.AddSentry();
        });
}