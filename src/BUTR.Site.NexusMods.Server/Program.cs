using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Utils;

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
using System.IO;
using System.Threading.Tasks;

using Z.EntityFramework.Extensions;

namespace BUTR.Site.NexusMods.Server;

public static class Program
{
    private static void PreBulkSaveChanges(DbContext context)
    {
        if (context is AppDbContextRead)
            throw new NotSupportedException("Write operations not supported with 'AppDbContextRead'!");
    }
    private static void PreBulkOperation(DbContext context, object o) => PreBulkSaveChanges(context);

    public static async Task Main(string[] args)
    {
        EntityFrameworkManager.PreBulkInsert = PreBulkOperation;
        EntityFrameworkManager.PreBulkDelete = PreBulkOperation;
        EntityFrameworkManager.PreBulkMerge  = PreBulkOperation;
        EntityFrameworkManager.PreBulkUpdate = PreBulkOperation;
        EntityFrameworkManager.PreBulkSynchronize = PreBulkOperation;
        EntityFrameworkManager.PreBulkSaveChanges = PreBulkSaveChanges;
        EntityFrameworkManager.ContextFactory = context => context switch
        {
            AppDbContextRead appDbContextRead => appDbContextRead.Create(),
            AppDbContextWrite appDbContextWrite => appDbContextWrite.Create(),
            _ => null
        };

        // I need to perform some cleanup at the start of the app
        foreach (var sourceFile in Directory.EnumerateFiles("scripts"))
            ScriptHandler.CompileAndExecute(Path.GetFileNameWithoutExtension(sourceFile), await File.ReadAllTextAsync(sourceFile));

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
            if (oltpSection is not null)
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

                var metricsEndpoint = oltpSection.GetValue<string>("MetricsEndpoint");
                if (metricsEndpoint is not null)
                {
                    var metricsProtocol = oltpSection.GetValue<OtlpExportProtocol>("MetricsProtocol");
                    openTelemetry.WithMetrics(builder => builder
                        .AddProcessInstrumentation()
                        .AddRuntimeInstrumentation(instrumentationOptions =>
                        {
                            
                        })
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation(instrumentationOptions =>
                        {
                            
                        })
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
            if (oltpSection is null) return;
            
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