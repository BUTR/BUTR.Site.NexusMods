using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Quartz;

using System;
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
            
            var oltpSection = ctx.Configuration.GetSection("Oltp");
            if (oltpSection is not null)
            {
                var openTelemetry = services.AddOpenTelemetry()
                    .ConfigureResource(builder => builder
                        .AddService(serviceName: typeof(Program).Assembly.GetName().Name!));

                var metricsEndpoint = oltpSection.GetValue<string>("MetricsEndpoint");
                if (metricsEndpoint is not null)
                {
                    var metricsProtocol = oltpSection.GetValue<OtlpExportProtocol>("MetricsProtocol");
                    openTelemetry.WithMetrics(builder => builder
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(metricsEndpoint);
                            o.Protocol = metricsProtocol;
                        }));
                }

                var tracingEndpoint = oltpSection.GetValue<string>("TracingEndpoint");
                if (tracingEndpoint is not null)
                {
                    var tracingProtocol = oltpSection.GetValue<OtlpExportProtocol>("TracingProtocol");
                    openTelemetry.WithTracing(builder => builder
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddNpgsql()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddQuartzInstrumentation()
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(tracingEndpoint);
                            o.Protocol = tracingProtocol;
                        }));
                }
            }
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            webBuilder.UseSentry();
        })
        .ConfigureLogging((ctx, builder) =>
        {
            builder.AddSentry();
            var oltpSection = ctx.Configuration.GetSection("Oltp");
            if (oltpSection is null) return;
            
            var loggingEndpoint = oltpSection.GetValue<string>("LoggingEndpoint");
            if (loggingEndpoint is null) return;
            var loggingProtocol = oltpSection.GetValue<OtlpExportProtocol>("LoggingProtocol");
            
            builder.AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.ParseStateValues = true;
                o.IncludeFormattedMessage = true;
                o.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(loggingEndpoint);
                    opt.Protocol = loggingProtocol;
                });
            });
        });
}