using BUTR.CrashReportViewer.Server.Helpers;
using BUTR.CrashReportViewer.Server.Services;
using BUTR.NexusMods.Server.Core.Extensions;
using BUTR.NexusMods.Server.Core.Options;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading;

namespace BUTR.CrashReportViewer.Server
{
    public class Startup
    {
        private const string JwtSectionName = "Jwt";
        private const string AuthenticationSectionName = "Authentication";

        private static JsonSerializerOptions Configure(JsonSerializerOptions opt)
        {
            opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.PropertyNameCaseInsensitive = true;
            opt.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            opt.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return opt;
        }

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

            services.AddSingleton<SqlHelperInit>();
            services.AddSingleton<SqlHelperMods>();
            services.AddSingleton<SqlHelperCrashReports>();
            services.AddSingleton<SqlHelperUserCrashReports>();

            services.AddHostedService<SqlService>();

            services.AddServerCore(Configuration, JwtSectionName);

            services.AddControllers().AddServerCore().AddJsonOptions(opt => Configure(opt.JsonSerializerOptions));
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