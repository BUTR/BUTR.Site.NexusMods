using Aragas.Extensions.Options.FluentValidation.Extensions;

using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Extensions;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Jobs;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils.Http.Logging;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

using Quartz;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace BUTR.Site.NexusMods.Server
{
    public sealed class Startup
    {
        private const string ConnectionStringsSectionName = "ConnectionStrings";
        private const string CrashReporterSectionName = "CrashReporter";
        private const string NexusModsSectionName = "NexusMods";
        private const string JwtSectionName = "Jwt";

        private static JsonSerializerOptions Configure(JsonSerializerOptions opt)
        {
            opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.PropertyNameCaseInsensitive = true;
            opt.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            opt.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return opt;
        }

        private readonly IConfiguration _configuration;
        private readonly AssemblyName? _assemblyName = Assembly.GetEntryAssembly()?.GetName();

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var userAgent = $"{_assemblyName?.Name ?? "ERROR"} v{_assemblyName?.Version?.ToString() ?? "ERROR"} (github.com/BUTR)";

            var connectionStringSection = _configuration.GetSection(ConnectionStringsSectionName);
            var crashReporterSection = _configuration.GetSection(CrashReporterSectionName);
            var nexusModsSection = _configuration.GetSection(NexusModsSectionName);
            var jwtSection = _configuration.GetSection(JwtSectionName);

            services.AddValidatedOptions<ConnectionStringsOptions, ConnectionStringsOptionsValidator>(connectionStringSection);
            services.AddValidatedOptionsWithHttp<CrashReporterOptions, CrashReporterOptionsValidator>(crashReporterSection);
            services.AddValidatedOptionsWithHttp<NexusModsOptions, NexusModsOptionsValidator>(nexusModsSection);
            services.AddValidatedOptions<JwtOptions, JwtOptionsValidator>(jwtSection);

            services.AddHttpClient(string.Empty).ConfigureHttpClient((sp, client) =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });
            services.AddHttpClient<NexusModsClient>().ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<NexusModsOptions>>().Value;
                client.BaseAddress = new Uri(opts.Endpoint);
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });
            services.AddHttpClient<NexusModsAPIClient>().ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<NexusModsOptions>>().Value;
                client.BaseAddress = new Uri(opts.APIEndpoint);
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });
            services.AddHttpClient<CrashReporterClient>().ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<CrashReporterOptions>>().Value;
                client.BaseAddress = new Uri(opts.Endpoint);
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{opts.Username}:{opts.Password}")));
            });

            services.AddQuartz(opt =>
            {
                opt.UseMicrosoftDependencyInjectionJobFactory();

                opt.AddJobAtStartup<CrashReportVersionProcessorJob>();
                opt.AddJob<CrashReportProcessorJob>(CronScheduleBuilder.CronSchedule("0 0 * * * ?").InTimeZone(TimeZoneInfo.Utc));
                opt.AddJob<NexusModsModFileUpdatesProcessorJob>(CronScheduleBuilder.CronSchedule("0 0 0 * * ?").InTimeZone(TimeZoneInfo.Utc));
                //opt.AddJob<NexusModsModFileProcessorJob>(CronScheduleBuilder.CronSchedule("0 0 0 * * ?").InTimeZone(TimeZoneInfo.Utc));
                opt.AddJob<NexusModsArticleProcessorJob>(CronScheduleBuilder.CronSchedule("0 0 0 * * ?").InTimeZone(TimeZoneInfo.Utc));
            });

            services.AddDbContext<AppDbContext>(x => x.UseNpgsql(_configuration.GetConnectionString("Main"), opt => opt.EnableRetryOnFailure()).AddPrepareInterceptorr());

            services.AddNexusModsDefaultServices();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, SyncLoggingHttpMessageHandlerBuilderFilter>());
            services.AddTransient<NexusModsInfo>();
            services.AddSingleton<DiffProvider>();

            services.AddAuthentication(ButrNexusModsAuthSchemeConstants.AuthScheme).AddNexusMods(options =>
            {
                var opts = jwtSection.Get<JwtOptions>();
                options.EncryptionKey = opts.EncryptionKey;
            });

            services.AddControllers().AddJsonOptions(opt => Configure(opt.JsonSerializerOptions));
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
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

                var currentAssembly = Assembly.GetExecutingAssembly();
                var xmlFilePaths = currentAssembly.GetReferencedAssemblies()
                    .Append(currentAssembly.GetName())
                    .Select(x => Path.Combine(Path.GetDirectoryName(currentAssembly.Location)!, $"{x.Name}.xml"))
                    .Where(File.Exists)
                    .ToList();
                foreach (var xmlFilePath in xmlFilePaths)
                    opt.IncludeXmlComments(xmlFilePath);
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

                options.ConnectionString = opts.Main;
                options.SchemaName = "public";
                options.TableName = "nexusmods_cache_entry";
                options.CreateInfrastructure = true;
            });
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseCors("Development");
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
            app.UseMetadata();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}