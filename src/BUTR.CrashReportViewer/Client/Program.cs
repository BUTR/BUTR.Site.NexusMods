using Blazored.LocalStorage;
using Blazored.SessionStorage;

using BUTR.CrashReportViewer.Client.Extensions;
using BUTR.CrashReportViewer.Client.Helpers;
using BUTR.CrashReportViewer.Client.Options;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Reflection;
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

                var assemblyName = Assembly.GetEntryAssembly()?.GetName();
                var userAgent = $"{assemblyName?.Name} v{assemblyName?.Version}";
                services.AddScoped(_ => new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
                    DefaultRequestHeaders =
                    {
                        {"User-Agent", userAgent}
                    }
                });
                services.AddHttpClient("InternalReports", (sp, client) =>
                {
                    client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}reports/");
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                });
                services.AddHttpClient("Backend", (sp, client) =>
                {
                    var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                    client.BaseAddress = new Uri(backendOptions.Endpoint);
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                });
                services.AddHttpClient("CrashReporter", (sp, client) =>
                {
                    var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                    client.BaseAddress = new Uri($"{backendOptions.Endpoint}/Reports/");
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                });

                services.AddScoped<BackendAPIClient>();

                services.AddBlazoredLocalStorage();
                services.AddBlazoredSessionStorage();

                services.AddAuthorizationCore();
                services.AddScoped<AuthenticationStateProvider, SimpleAuthenticationStateProvider>();
            });
    }
}