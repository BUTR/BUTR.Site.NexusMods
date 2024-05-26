using Aragas.Extensions.Options.FluentValidation.Extensions;

using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Extensions;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Contexts.Configs;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Jobs;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Repositories;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;
using BUTR.Site.NexusMods.Server.Utils.Csv.Extensions;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;
using BUTR.Site.NexusMods.Server.Utils.Http.Logging;
using BUTR.Site.NexusMods.Server.Utils.Http.StreamingMultipartResults;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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

public sealed partial class Startup
{
    private const string ConnectionStringsSectionName = "ConnectionStrings";
    private const string CrashReporterSectionName = "CrashReporter";
    private const string NexusModsSectionName = "NexusMods";
    private const string NexusModsUsersSectionName = "NexusModsUsers";
    private const string JwtSectionName = "Jwt";
    private const string GitHubSectionName = "GitHub";
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

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    partial void ConfigureServicesPartial(IServiceCollection services);

    public void ConfigureServices(IServiceCollection services)
    {
        var assemblyName = typeof(Startup).Assembly.GetName();
        var userAgent = $"{assemblyName.Name ?? "ERROR"} v{assemblyName.Version?.ToString() ?? "ERROR"} (github.com/BUTR)";

        var connectionStringSection = _configuration.GetSection(ConnectionStringsSectionName);
        var crashReporterSection = _configuration.GetSection(CrashReporterSectionName);
        var nexusModsSection = _configuration.GetSection(NexusModsSectionName);
        var nexusModsUsersSection = _configuration.GetSection(NexusModsUsersSectionName);
        var jwtSection = _configuration.GetSection(JwtSectionName);
        var gitHubSection = _configuration.GetSection(GitHubSectionName);
        var discordSection = _configuration.GetSection(DiscordSectionName);
        var steamAPISection = _configuration.GetSection(SteamAPISectionName);
        var depotDownloaderSection = _configuration.GetSection(DepotDownloaderSectionName);

        services.AddOptions<JsonSerializerOptions>().Configure(opt => Configure(opt));
        services.AddValidatedOptions<ConnectionStringsOptions, ConnectionStringsOptionsValidator>().Bind(connectionStringSection);
        services.AddValidatedOptionsWithHttp<CrashReporterOptions, CrashReporterOptionsValidator>().Bind(crashReporterSection);
        services.AddValidatedOptionsWithHttp<NexusModsOptions, NexusModsOptionsValidator>().Bind(nexusModsSection);
        services.AddValidatedOptionsWithHttp<NexusModsUsersOptions, NexusModsUsersOptionsValidator>().Bind(nexusModsUsersSection);
        services.AddValidatedOptions<JwtOptions, JwtOptionsValidator>().Bind(jwtSection);
        services.AddValidatedOptions<GitHubOptions, GitHubOptionsValidator>().Bind(gitHubSection);
        services.AddValidatedOptions<DiscordOptions, DiscordOptionsValidator>().Bind(discordSection);
        services.AddValidatedOptions<SteamAPIOptions, SteamAPIOptionsValidator>().Bind(steamAPISection);
        services.AddValidatedOptions<SteamDepotDownloaderOptions, SteamDepotDownloaderOptionsValidator>().Bind(depotDownloaderSection);

        services.AddHttpClient(string.Empty).ConfigureHttpClient((_, client) =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<INexusModsClient, NexusModsClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://nexusmods.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<INexusModsAPIClient, NexusModsAPIClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://api.nexusmods.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<INexusModsAPIv2Client, NexusModsAPIv2Client>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://api.nexusmods.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<INexusModsUsersClient, NexusModsUsersClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://users.nexusmods.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<ICrashReporterClient, CrashReporterClient>().ConfigureHttpClient((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<CrashReporterOptions>>().Value;
            client.BaseAddress = new Uri(opts.Endpoint);
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{opts.Username}:{opts.Password}")));
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IGitHubClient, GitHubClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://github.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IGitHubAPIClient, GitHubAPIClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IDiscordClient, DiscordClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://discord.com/api/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<ISteamCommunityClient, SteamCommunityClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://steamcommunity.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<ISteamAPIClient, SteamAPIClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://api.steampowered.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IGOGAuthClient, GOGAuthClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://auth.gog.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IGOGEmbedClient, GOGEmbedClient>().ConfigureHttpClient((_, client) =>
        {
            client.BaseAddress = new Uri("https://embed.gog.com/");
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());

        services.AddQuartz(opt =>
        {
            opt.AddJobListener<IQuartzEventProviderService>(sp => sp.GetRequiredService<IQuartzEventProviderService>());
            opt.AddTriggerListener<IQuartzEventProviderService>(sp => sp.GetRequiredService<IQuartzEventProviderService>());

            string AtEveryFirstDayOfMonth(int hour = 0) => $"0 0 {hour} ? 1-12 * *";
            string AtEveryMonday(int hour = 0) => $"0 0 {hour} ? * MON *";
            string AtEveryDay(int hour = 0) => $"0 0 {hour} ? * *";
            string AtEveryHour() => "0 0 * * * ?";
#if DEBUG
            //opt.AddJobAtStartup<CrashReportProcessor2Job>();
            /*
            opt.AddJob<CrashReportProcessorJob>();
            opt.AddJob<CrashReportAnalyzerProcessorJob>();
            opt.AddJob<TopExceptionsTypesAnalyzerProcessorJob>();
            opt.AddJobAtStartup<AutocompleteProcessorProcessorJob>();
            opt.AddJob<NexusModsModFileProcessorJob>();
            //opt.AddJobAtStartup<NexusModsModFileProcessorJob>();
            opt.AddJob<NexusModsModFileUpdatesProcessorJob>();
            opt.AddJob<NexusModsArticleProcessorJob>();
            opt.AddJob<NexusModsArticleUpdatesProcessorJob>();
            */
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



        var types = typeof(Startup).Assembly.GetTypes().Where(x => x is { IsAbstract: false, BaseType: { IsGenericType: true } }).ToList();
        foreach (var type in types.Where(x => x.BaseType!.GetGenericTypeDefinition() == typeof(BaseEntityConfigurationWithTenant<>)))
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IEntityConfiguration), type));
        foreach (var type in types.Where(x => x.BaseType!.GetGenericTypeDefinition() == typeof(BaseEntityConfiguration<>)))
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IEntityConfiguration), type));

        services.AddDbContext<BaseAppDbContext>(ServiceLifetime.Scoped);
        services.AddDbContextFactory<AppDbContextRead>(lifetime: ServiceLifetime.Scoped);
        services.AddDbContextFactory<AppDbContextWrite>(lifetime: ServiceLifetime.Scoped);

        services.AddNexusModsDefaultServices();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, SyncLoggingHttpMessageHandlerBuilderFilter>());

        services.AddAuthentication(ButrNexusModsAuthSchemeConstants.AuthScheme).AddNexusMods(options =>
        {
            var opts = jwtSection.Get<JwtOptions>();
            options.EncryptionKey = opts?.EncryptionKey ?? string.Empty;
        });

        services.AddStreamingMultipartResult();

        services.AddHttpContextAccessor();
        services.AddRouting(opt =>
        {
            opt.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
        });
        services.AddControllersWithAPIResult(opt =>
        {
            opt.Conventions.Add(new SlugifyActionConvention());
            opt.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));

            opt.AddCsvOutputFormatters();

            opt.ValueProviderFactories.Add(new ClaimsValueProviderFactory());
        }).AddJsonOptions(opt => Configure(opt.JsonSerializerOptions));
        services.AddResponseCompression(opt =>
        {
            opt.Providers.Add<BrotliCompressionProvider>();
            opt.Providers.Add<GzipCompressionProvider>();
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

            opt.OperationFilter<SwaggerOperationIdFilter>();
            opt.DescribeAllParametersInCamelCase();
            opt.SupportNonNullableReferenceTypes();
            opt.SchemaFilter<RequiredMemberFilter>();
            opt.SchemaFilter<NullableFilter>();
            opt.OperationFilter<BindIgnoreFilter>();
            opt.OperationFilter<AuthResponsesOperationFilter>();
            opt.OperationFilter<ApiResultOperationFilter>();
            opt.SchemaFilter<VogenSchemaFilter>();
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
                opt.IncludeXmlComments(xmlFilePath, true);
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.AddResponseCaching();

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

        ConfigureServicesPartial(services);
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