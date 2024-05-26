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
using OpenTelemetry.ResourceDetectors.Container;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Quartz;

using Serilog;
using Serilog.Events;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting web application");

            var host = CreateHostBuilder(args).Build();

            await host.SeedDbContext<BaseAppDbContext>().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
            services.AddQuartzHostedService(options =>
            {
                options.AwaitApplicationStarted = true;
                options.WaitForJobsToComplete = true;
            });

            if (ctx.Configuration.GetSection("Oltp") is { } oltpSection)
            {
                var openTelemetry = services.AddOpenTelemetry()
                    .ConfigureResource(builder =>
                    {
                        builder.AddDetector(new ContainerResourceDetector());
                        builder.AddService(
                            ctx.HostingEnvironment.ApplicationName,
                            ctx.HostingEnvironment.EnvironmentName,
                            typeof(Program).Assembly.GetName().Version?.ToString(),
                            false,
                            Environment.MachineName);
                        builder.AddTelemetrySdk();
                    });

                if (oltpSection.GetValue<string?>("MetricsEndpoint") is { } metricsEndpoint)
                {
                    var metricsProtocol = oltpSection.GetValue<OtlpExportProtocol>("MetricsProtocol");
                    openTelemetry.WithMetrics(builder => builder
                        .AddProcessInstrumentation()
                        .AddRuntimeInstrumentation(instrumentationOptions =>
                        {

                        })
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(metricsEndpoint);
                            o.Protocol = metricsProtocol;
                        }));
                }

                if (oltpSection.GetValue<string?>("TracingEndpoint") is { } tracingEndpoint)
                {
                    var tracingProtocol = oltpSection.GetValue<OtlpExportProtocol>("TracingProtocol");
                    openTelemetry.WithTracing(builder => builder
                        .AddEntityFrameworkCoreInstrumentation(instrumentationOptions =>
                        {
                            instrumentationOptions.SetDbStatementForText = true;
                        })
                        .AddNpgsql(instrumentationOptions =>
                        {

                        })
                        .AddGrpcClientInstrumentation(instrumentationOptions =>
                        {

                        })
                        .AddHttpClientInstrumentation(instrumentationOptions =>
                        {
                            instrumentationOptions.RecordException = true;
                        })
                        .AddAspNetCoreInstrumentation(instrumentationOptions =>
                        {
                            instrumentationOptions.RecordException = true;
                        })
                        .AddQuartzInstrumentation(instrumentationOptions =>
                        {
                            instrumentationOptions.RecordException = true;
                        })
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
        })
        .UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);
        }, writeToProviders: true)
        .ConfigureLogging((ctx, builder) =>
        {
            var oltpSection = ctx.Configuration.GetSection("Oltp");
            if (oltpSection == null!) return;

            var loggingEndpoint = oltpSection.GetValue<string>("LoggingEndpoint");
            if (loggingEndpoint is null) return;
            var loggingProtocol = oltpSection.GetValue<OtlpExportProtocol>("LoggingProtocol");

            builder.AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.ParseStateValues = true;
                o.IncludeFormattedMessage = true;
                o.AddOtlpExporter((options, processorOptions) =>
                {
                    options.Endpoint = new Uri(loggingEndpoint);
                    options.Protocol = loggingProtocol;
                });
            });
        });
}