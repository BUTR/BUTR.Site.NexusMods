using BUTR.CrashReportViewer.Server.Helpers;
using BUTR.CrashReportViewer.Server.Options;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace BUTR.CrashReportViewer.Server
{
    public class Startup
    {
        private const string JwtSectionName = "Jwt";
        private const string AuthenticationSectionName = "Authentication";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtOptions>(Configuration.GetSection(JwtSectionName));
            services.Configure<AuthenticationOptions>(Configuration.GetSection(AuthenticationSectionName));

            var assemblyName = Assembly.GetEntryAssembly()?.GetName();
            var userAgent = $"{assemblyName?.Name ?? "BUTR.CrashReportViewer.Server"} v{Assembly.GetEntryAssembly()?.GetName().Version}";
            services.AddHttpClient("NexusModsAPI", client =>
            {
                client.BaseAddress = new Uri("https://api.nexusmods.com/");
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });
            services.AddHttpClient("CrashReporter", client =>
            {
                client.BaseAddress = new Uri("https://crash.butr.dev/report/");
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });

            services.AddScoped<NexusModsAPIClient>();
            services.AddSingleton<SqlHelperInit>();
            services.AddSingleton<SqlHelperMods>();
            services.AddSingleton<SqlHelperCrashReports>();
            services.AddSingleton<SqlHelperUserCrashReports>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtOptions = Configuration.GetSection(JwtSectionName).Get<JwtOptions>();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = false,
                        ValidateActor = false,
                        ValidateTokenReplay = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SignKey)),
                        TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.EncryptionKey)),
                        ClockSkew = TimeSpan.FromMinutes(5),
                    };
                });

            services.AddControllers().AddJsonOptions(opts => {
                opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
            services.AddRazorPages();

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
                options.ConnectionString = Configuration.GetConnectionString("Main");
                options.SchemaName = "public";
                options.TableName = "nexusmods_cache_entry";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SqlHelperInit sqlHelperInit)
        {
            _ = sqlHelperInit.CreateTablesIfNotExistAsync(CancellationToken.None);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("Development");
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}