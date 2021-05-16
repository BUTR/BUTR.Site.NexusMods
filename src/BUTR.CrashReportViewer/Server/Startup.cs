using AspNetCore.Proxy;

using BUTR.CrashReportViewer.Shared.Contexts;
using BUTR.CrashReportViewer.Shared.Helpers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;

namespace BUTR.CrashReportViewer.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("NexusModsAPI", client =>
            {
                client.BaseAddress = new Uri("https://api.nexusmods.com/");
            });

            services.AddScoped<NexusModsAPIClient>();

            services.AddDbContext<MainDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("Main")));

            services.AddProxies();

            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("DevCorsPolicy", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
                options.AddPolicy("GitHubPages", builder =>
                {
                    builder
                        .WithOrigins("https://crashreports.bannerlord.aragas.org")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<MainDbContext>().Database.EnsureCreated();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("DevCorsPolicy");
            }
            else
            {
                //app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
                app.UseCors("GitHubPages");
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseProxies(proxies =>
            {
                proxies.Map("NexusModsAPIProxy/v1/users/validate.json",
                    proxy => proxy.UseHttp("https://api.nexusmods.com/v1/users/validate.json"));
                proxies.Map("NexusModsAPIProxy/v1/games/{game_domain_name}/mods/{id}.json",
                    proxy => proxy.UseHttp((_, args) => $"https://api.nexusmods.com/v1/games/{args["game_domain_name"]}/mods/{args["id"]}.json"));
            });
        }
    }
}
