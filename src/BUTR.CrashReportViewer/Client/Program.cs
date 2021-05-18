using Blazored.LocalStorage;

using BUTR.CrashReportViewer.Client.Options;
using BUTR.CrashReportViewer.Shared.Helpers;

using Flurl;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BUTR.CrashReportViewer.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.Configure<BackendOptions>(builder.Configuration.GetSection("Backend"));

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });
            builder.Services.AddHttpClient("NexusModsAPI", (sp, client) =>
            {
                var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                client.BaseAddress = new Uri(Url.Combine($"{backendOptions.Endpoint}", "/NexusModsAPIProxy/"));
            });
            builder.Services.AddHttpClient("Backend", (sp, client) =>
            {
                var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                client.BaseAddress = new Uri(backendOptions.Endpoint);
            });

            builder.Services.AddScoped<NexusModsAPIClient>();

            builder.Services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync();
        }
    }
}