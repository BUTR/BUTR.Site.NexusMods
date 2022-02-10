using BUTR.Authentication.NexusMods.Authentication;
using BUTR.Authentication.NexusMods.Extensions;
using BUTR.Site.NexusMods.Server.Options;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Services.Database;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace BUTR.Site.NexusMods.Server
{
    public class Startup
    {
        private const string ConnectionStringsSectionName = "ConnectionStrings";
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
            var urlsSection = _configuration.GetSection(UrlsSectionName);
            var jwtSection = _configuration.GetSection(JwtSectionName);

            services.Configure<ConnectionStringsOptions>(connectionStringSection);
            services.Configure<ServiceUrlsOptions>(urlsSection);
            services.Configure<JwtOptions>(jwtSection);

            services.AddHttpClient("NexusModsAPI").ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ServiceUrlsOptions>>().Value;
                client.BaseAddress = new Uri(opts.NexusMods);
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });
            services.AddHttpClient("CrashReporter").ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<ServiceUrlsOptions>>().Value;
                client.BaseAddress = new Uri(opts.CrashReporter);
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });

            services.AddSingleton<SeederProvider>();
            services.AddSingleton<MainConnectionProvider>();
            services.AddSingleton<ModsProvider>();
            services.AddSingleton<ModListProvider>();
            services.AddSingleton<CrashReportsProvider>();
            services.AddSingleton<UserCrashReportsProvider>();
            services.AddSingleton<RoleProvider>();

            services.AddHostedService<SeederService>();

            //services.AddServerCore(_configuration, JwtSectionName);
            services.AddScoped<NexusModsAPIClient>();

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

                var xmlFilename = $"{_assemblyName?.Name ?? "ERROR"}.xml";
                opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
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