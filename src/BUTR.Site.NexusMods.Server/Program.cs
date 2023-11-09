using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server;

public static class Program
{
    private static void PreBulkSaveChanges(DbContext context)
    {
        if (context is AppDbContextRead)
            AppDbContextRead.WriteNotSupported();
    }
    private static void PreBulkOperation(DbContext context, object o) => PreBulkSaveChanges(context);

    public static async Task Main(string[] args)
    {
        Z.EntityFramework.Extensions.EntityFrameworkManager.PreBulkInsert = PreBulkOperation;
        Z.EntityFramework.Extensions.EntityFrameworkManager.PreBulkDelete = PreBulkOperation;
        Z.EntityFramework.Extensions.EntityFrameworkManager.PreBulkMerge  = PreBulkOperation;
        Z.EntityFramework.Extensions.EntityFrameworkManager.PreBulkUpdate = PreBulkOperation;
        Z.EntityFramework.Extensions.EntityFrameworkManager.PreBulkSynchronize = PreBulkOperation;
        Z.EntityFramework.Extensions.EntityFrameworkManager.PreBulkSaveChanges = PreBulkSaveChanges;
        Z.EntityFramework.Extensions.EntityFrameworkManager.ContextFactory = context => context switch
        {
            AppDbContextRead appDbContextRead => appDbContextRead.New(),
            AppDbContextWrite appDbContextWrite => appDbContextWrite.New(),
            _ => null
        };

        var host = CreateHostBuilder(args).Build();

        await host.SeedDbContext<BaseAppDbContext>().RunAsync();
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

            var oltpSection = ctx.Configuration.GetSection("Oltp");
            if (oltpSection != null!)
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

                var metricsEndpoint = oltpSection.GetValue<string?>("MetricsEndpoint");
                if (metricsEndpoint is not null)
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

                var tracingEndpoint = oltpSection.GetValue<string?>("TracingEndpoint");
                if (tracingEndpoint is not null)
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