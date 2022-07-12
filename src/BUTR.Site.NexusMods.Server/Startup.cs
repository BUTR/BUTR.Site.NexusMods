using Aragas.Extensions.Options.FluentValidation.Extensions;

using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Extensions;
using BUTR.Site.NexusMods.Server.Contexts;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

using System;
using System.IO;
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
        private const string UrlsSectionName = "Urls";
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
            var userAgent = $"{_assemblyName?.Name ?? "ERROR"} v{_assemblyName?.Version?.ToString() ?? "ERROR"}";

            var connectionStringSection = _configuration.GetSection(ConnectionStringsSectionName);
            var crashReporterSection = _configuration.GetSection(CrashReporterSectionName);
            var urlsSection = _configuration.GetSection(UrlsSectionName);
            var jwtSection = _configuration.GetSection(JwtSectionName);

            services.AddValidatedOptions<ConnectionStringsOptions, ConnectionStringsOptionsValidator>(connectionStringSection);
            services.AddValidatedOptionsWithHttp<CrashReporterOptions, CrashReporterOptionsValidator>(crashReporterSection);
            services.AddValidatedOptionsWithHttp<ServiceUrlsOptions, ServiceUrlsOptionsValidator>(urlsSection);
            services.AddValidatedOptions<JwtOptions, JwtOptionsValidator>(jwtSection);

            services.AddHttpClient<NexusModsAPIClient>().ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ServiceUrlsOptions>>().Value;
                client.BaseAddress = new Uri(opts.NexusMods);
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

            services.AddDbContext<AppDbContext>(x => x.UseNpgsql(_configuration.GetConnectionString("Main")));

            services.AddHostedService<CrashReportHandler>();

            services.AddNexusModsDefaultServices();

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
}