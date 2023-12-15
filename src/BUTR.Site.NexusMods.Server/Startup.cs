using Aragas.Extensions.Options.FluentValidation.Extensions;

using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Extensions;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Contexts.Configs;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Jobs;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Server.Utils.Http.Logging;
using BUTR.Site.NexusMods.Server.Utils.Http.StreamingMultipartResults;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;

using Quartz;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace BUTR.Site.NexusMods.Server;

public sealed class Startup
{
    private const string ConnectionStringsSectionName = "ConnectionStrings";
    private const string CrashReporterSectionName = "CrashReporter";
    private const string NexusModsSectionName = "NexusMods";
    private const string JwtSectionName = "Jwt";
    private const string DiscordSectionName = "Discord";
    private const string SteamAPISectionName = "SteamAPI";
    private const string DepotDownloaderSectionName = "DepotDownloader";

    private static JsonSerializerOptions Configure(JsonSerializerOptions opt)
    {
        opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opt.PropertyNameCaseInsensitive = true;
        opt.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        opt.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return opt;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrTransientHttpStatusCode()
            .WaitAndRetryAsync(delay);
    }

    private readonly IConfiguration _configuration;
    private readonly AssemblyName _assemblyName = typeof(Startup).Assembly.GetName();

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var userAgent = $"{_assemblyName.Name ?? "ERROR"} v{_assemblyName.Version?.ToString() ?? "ERROR"} (github.com/BUTR)";

        var connectionStringSection = _configuration.GetSection(ConnectionStringsSectionName);
        var crashReporterSection = _configuration.GetSection(CrashReporterSectionName);
        var nexusModsSection = _configuration.GetSection(NexusModsSectionName);
        var jwtSection = _configuration.GetSection(JwtSectionName);
        var discordSection = _configuration.GetSection(DiscordSectionName);
        var steamAPISection = _configuration.GetSection(SteamAPISectionName);
        var depotDownloaderSection = _configuration.GetSection(DepotDownloaderSectionName);

        services.AddOptions<JsonSerializerOptions>().Configure(opt => Configure(opt));
        services.AddValidatedOptions<ConnectionStringsOptions, ConnectionStringsOptionsValidator>().Bind(connectionStringSection);
        services.AddValidatedOptionsWithHttp<CrashReporterOptions, CrashReporterOptionsValidator>().Bind(crashReporterSection);
        services.AddValidatedOptionsWithHttp<NexusModsOptions, NexusModsOptionsValidator>().Bind(nexusModsSection);
        services.AddValidatedOptions<JwtOptions, JwtOptionsValidator>().Bind(jwtSection);
        services.AddValidatedOptions<DiscordOptions, DiscordOptionsValidator>().Bind(discordSection);
        services.AddValidatedOptions<SteamAPIOptions, SteamAPIOptionsValidator>().Bind(steamAPISection);
        services.AddValidatedOptions<SteamDepotDownloaderOptions, SteamDepotDownloaderOptionsValidator>().Bind(depotDownloaderSection);

        services.AddHttpClient(string.Empty).ConfigureHttpClient((_, client) =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<NexusModsClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://nexusmods.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<NexusModsAPIClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://api.nexusmods.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<CrashReporterClient>().ConfigureHttpClient((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<CrashReporterOptions>>().Value;
            client.BaseAddress = new Uri(opts.Endpoint);
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{opts.Username}:{opts.Password}")));
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<DiscordClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://discord.com/api/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<SteamCommunityClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://steamcommunity.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<SteamAPIClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://api.steampowered.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<GOGAuthClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://auth.gog.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<GOGEmbedClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://embed.gog.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());

        services.AddSingleton<SteamBinaryCache>();
        services.AddSingleton<SteamDepotDownloader>();

        services.AddSingleton<QuartzEventProviderService>();
        services.AddHostedService<QuartzListenerBackgroundService>();
        services.AddQuartz(opt =>
        {
            opt.AddJobListener<QuartzEventProviderService>(sp => sp.GetRequiredService<QuartzEventProviderService>());
            opt.AddTriggerListener<QuartzEventProviderService>(sp => sp.GetRequiredService<QuartzEventProviderService>());

            string AtEveryFirstDayOfMonth(int hour = 0) => $"0 0 {hour} ? 1-12 * *";
            string AtEveryMonday(int hour = 0) => $"0 0 {hour} ? * MON *";
            string AtEveryDay(int hour = 0) => $"0 0 {hour} ? * *";
            string AtEveryHour() => "0 0 * * * ?";
#if DEBUG
            opt.AddJob<CrashReportProcessorJob>();
            opt.AddJob<CrashReportAnalyzerProcessorJob>();
            opt.AddJob<TopExceptionsTypesAnalyzerProcessorJob>();
            opt.AddJobAtStartup<AutocompleteProcessorProcessorJob>();
            opt.AddJob<NexusModsModFileProcessorJob>();
            //opt.AddJobAtStartup<NexusModsModFileProcessorJob>();
            opt.AddJob<NexusModsModFileUpdatesProcessorJob>();
            opt.AddJob<NexusModsArticleProcessorJob>();
            opt.AddJob<NexusModsArticleUpdatesProcessorJob>();
#else
            // Hourly
            opt.AddJob<CrashReportProcessorJob>(CronScheduleBuilder.CronSchedule(AtEveryHour()).InTimeZone(TimeZoneInfo.Utc));
            opt.AddJob<AutocompleteProcessorProcessorJob>(CronScheduleBuilder.CronSchedule(AtEveryHour()).InTimeZone(TimeZoneInfo.Utc));
            opt.AddJob<TopExceptionsTypesAnalyzerProcessorJob>(CronScheduleBuilder.CronSchedule(AtEveryHour()).InTimeZone(TimeZoneInfo.Utc));
            opt.AddJob<CrashReportAnalyzerProcessorJob>(CronScheduleBuilder.CronSchedule(AtEveryHour()).InTimeZone(TimeZoneInfo.Utc));

            // Daily
            opt.AddJob<QuartzLogHistoryManagerExecutionLogsJob>(CronScheduleBuilder.CronSchedule(AtEveryDay(00)).InTimeZone(TimeZoneInfo.Utc));
            opt.AddJob<NexusModsModFileUpdatesProcessorJob>(CronScheduleBuilder.CronSchedule(AtEveryDay(00)).InTimeZone(TimeZoneInfo.Utc));
            opt.AddJob<NexusModsArticleUpdatesProcessorJob>(CronScheduleBuilder.CronSchedule(AtEveryDay(12)).InTimeZone(TimeZoneInfo.Utc));

            // Monthly
            //opt.AddJob<NexusModsModFileProcessorJob>(CronScheduleBuilder.CronSchedule(AtEveryFirstDayOfMonth(06)).InTimeZone(TimeZoneInfo.Utc));
            //opt.AddJob<NexusModsArticleProcessorJob>(CronScheduleBuilder.CronSchedule(AtEveryFirstDayOfMonth(18)).InTimeZone(TimeZoneInfo.Utc));
#endif
        });

        services.AddMemoryCache();

        services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

        services.AddScoped<IEntityConfigurationFactory, EntityConfigurationFactory>();
        var types = typeof(Startup).Assembly.GetTypes().Where(x => x is { IsAbstract: false, BaseType: { IsGenericType: true } }).ToList();
        foreach (var type in types.Where(x => x.BaseType!.GetGenericTypeDefinition() == typeof(BaseEntityConfigurationWithTenant<>)))
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IEntityConfiguration), type));
        foreach (var type in types.Where(x => x.BaseType!.GetGenericTypeDefinition() == typeof(BaseEntityConfiguration<>)))
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IEntityConfiguration), type));

        services.AddSingleton<NpgsqlDataSourceProvider>();
        services.AddDbContext<BaseAppDbContext>(ServiceLifetime.Scoped);
        services.AddDbContextFactory<AppDbContextRead>(lifetime: ServiceLifetime.Scoped);
        services.AddDbContextFactory<AppDbContextWrite>(lifetime: ServiceLifetime.Scoped);
        services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();
        services.AddScoped<IAppDbContextRead>(sp => sp.GetRequiredService<IAppDbContextFactory>().CreateRead());
        services.AddScoped<IAppDbContextWrite>(sp => sp.GetRequiredService<IAppDbContextFactory>().CreateWrite());

        services.AddNexusModsDefaultServices();

        services.AddHostedService<DiscordLinkedRolesService>();
        services.AddScoped<IDiscordStorage, DatabaseDiscordStorage>();
        services.AddScoped<ISteamStorage, DatabaseSteamStorage>();
        services.AddScoped<IGOGStorage, DatabaseGOGStorage>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, SyncLoggingHttpMessageHandlerBuilderFilter>());
        services.AddTransient<CrashReportBatchedHandler>();
        services.AddTransient<NexusModsModFileParser>();
        services.AddSingleton<DiffProvider>();

        services.AddAuthentication(ButrNexusModsAuthSchemeConstants.AuthScheme).AddNexusMods(options =>
        {
            var opts = jwtSection.Get<JwtOptions>();
            options.EncryptionKey = opts?.EncryptionKey ?? string.Empty;
        });

        services.AddStreamingMultipartResult();

        services.AddControllersWithAPIResult(opt => opt.ValueProviderFactories.Add(new ClaimsValueProviderFactory()))
            .AddJsonOptions(opt => Configure(opt.JsonSerializerOptions));

        services.AddHttpContextAccessor();
        services.AddRouting();
        services.AddResponseCompression(options =>
        {
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.SmallestSize;
        });

        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "BUTR's NexusMods API",
                Description = "BUTR's API that uses NexusMods API Key for authentication",
            });

            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Scheme = ButrNexusModsAuthSchemeConstants.AuthScheme,
                BearerFormat = "Custom",
                Name = HeaderNames.Authorization,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = $"Put with the **{ButrNexusModsAuthSchemeConstants.AuthScheme}** prefix!",

                Reference = new OpenApiReference
                {
                    Id = ButrNexusModsAuthSchemeConstants.AuthScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };
            opt.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() }
            });

            opt.DescribeAllParametersInCamelCase();
            opt.SupportNonNullableReferenceTypes();
            opt.SchemaFilter<RequiredMemberFilter>();
            opt.OperationFilter<BindIgnoreFilter>();
            opt.OperationFilter<AuthResponsesOperationFilter>();
            opt.OperationFilter<ApiResultOperationFilter>();
            opt.ValueObjectFilter();
            opt.EnableAnnotations();
            opt.UseAllOfToExtendReferenceSchemas();

            // Really .NET?
            opt.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "time-span" });

            var currentAssembly = typeof(Startup).Assembly;
            var xmlFilePaths = currentAssembly.GetReferencedAssemblies()
                .Append(currentAssembly.GetName())
                .Select(x => Path.Combine(Path.GetDirectoryName(currentAssembly.Location)!, $"{x.Name}.xml"))
                .Where(File.Exists)
                .ToList();
            foreach (var xmlFilePath in xmlFilePaths)
                opt.IncludeXmlComments(xmlFilePath);
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.AddCors(options =>
        {
            options.AddPolicy("Development", builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
            );
        });

        services.AddDistributedPostgreSqlCache(options =>
        {
            var opts = connectionStringSection.Get<ConnectionStringsOptions>();

            options.ConnectionString = opts?.Main;
            options.SchemaName = "cache";
            options.TableName = "sitenexusmods_cache";
            options.CreateInfrastructure = true;
        });
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseCors("Development");
        }
        else
        {
            app.UseExceptionHandler();
        }

        app.UseResponseCompression();

        app.UseSwagger(opt =>
        {
            opt.RouteTemplate = "/api/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(opt =>
        {
            opt.RoutePrefix = "api";
            opt.SwaggerEndpoint("/api/v1/swagger.json", "BUTR's NexusMods API");
        });

        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}