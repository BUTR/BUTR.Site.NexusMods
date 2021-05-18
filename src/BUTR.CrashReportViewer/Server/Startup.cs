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
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

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
            services.AddRazorPages();

            services.AddCors(options =>
            {
                options.AddPolicy("Development", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<MainDbContext>().Database.EnsureCreated();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("Development");
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // CORS on Production is handled externally
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}