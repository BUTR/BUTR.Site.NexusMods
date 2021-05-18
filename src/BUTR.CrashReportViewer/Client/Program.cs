using Blazored.LocalStorage;

using BUTR.CrashReportViewer.Client.Extensions;
using BUTR.CrashReportViewer.Client.Options;
using BUTR.CrashReportViewer.Shared.Helpers;

using Flurl;

using Microsoft.AspNetCore.Components.Authorization;
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
        public static Task Main(string[] args) => CreateHostBuilder(args).Build().RunAsync();

        public static WebAssemblyHostBuilder CreateHostBuilder(string[] args) => WebAssemblyHostBuilder
            .CreateDefault(args)
            .AddRootComponent<App>("#app")
            .ConfigureServices((builder, services) =>
            {
                services.Configure<BackendOptions>(builder.Configuration.GetSection("Backend"));

                services.AddScoped(sp => new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                });
                services.AddHttpClient("NexusModsAPI", (sp, client) =>
                {
                    var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                    client.BaseAddress = new Uri(Url.Combine($"{backendOptions.Endpoint}", "/NexusModsAPIProxy/"));
                });
                services.AddHttpClient("Backend", (sp, client) =>
                {
                    var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                    client.BaseAddress = new Uri(backendOptions.Endpoint);
                });

                services.AddScoped<NexusModsAPIClient>();
                services.AddScoped<AuthenticationStateProvider, NexusModsAuthenticationStateProvider>();

                services.AddBlazoredLocalStorage();

                services.AddAuthorizationCore(options =>
                {

                });
            });
    }
}