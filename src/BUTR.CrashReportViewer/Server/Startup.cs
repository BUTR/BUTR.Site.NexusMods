using AspNetCore.Proxy;

using BUTR.CrashReportViewer.Server.Contexts;
using BUTR.CrashReportViewer.Server.Helpers;
using BUTR.CrashReportViewer.Server.Options;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BUTR.CrashReportViewer.Server
{
    public class Startup
    {
        private const string JwtSectionName = "Jwt";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtOptions>(Configuration.GetSection(JwtSectionName));


            services.AddHttpClient("NexusModsAPI", client =>
            {
                client.BaseAddress = new Uri("https://api.nexusmods.com/");
            });
            services.AddHttpClient("CrashReporter", client =>
            {
                client.BaseAddress = new Uri("https://crash.butr.dev/report/");
            });

            services.AddScoped<NexusModsAPIClient>();

            services.AddDbContext<MainDbContext>(o => o.UseNpgsql(Configuration.GetConnectionString("Main")));
            services.AddDbContext<Dummy1DbContext>(o => o.UseNpgsql(Configuration.GetConnectionString("Main")));
            services.AddDbContext<Dummy2DbContext>(o => o.UseNpgsql(Configuration.GetConnectionString("Main")));
            services.AddDbContext<Dummy3DbContext>(o => o.UseNpgsql(Configuration.GetConnectionString("Main")));
            services.AddDbContext<Dummy4DbContext>(o => o.UseNpgsql(Configuration.GetConnectionString("Main")));

            services.AddProxies();

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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                foreach (var type in new [] { typeof(Dummy1DbContext), typeof(Dummy2DbContext), typeof(Dummy3DbContext), typeof(Dummy4DbContext) })
                {
                    var dbContext = (DbContext) serviceScope.ServiceProvider.GetRequiredService(type);
                    dbContext.Database.EnsureCreated();
                    try
                    {
                        (dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator)?.CreateTables();
                    }
                    catch { }
                }
            }

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