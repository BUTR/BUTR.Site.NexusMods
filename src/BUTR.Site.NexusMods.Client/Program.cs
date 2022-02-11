using Blazored.LocalStorage;
using Blazored.SessionStorage;

using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise.Localization;

using BUTR.Site.NexusMods.Client.Extensions;
using BUTR.Site.NexusMods.Client.Models;
using BUTR.Site.NexusMods.Client.Options;
using BUTR.Site.NexusMods.Client.Services;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Client
{
    public static class Program
    {
        private static JsonSerializerOptions Configure(JsonSerializerOptions opt)
        {
            opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.PropertyNameCaseInsensitive = true;
            opt.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            opt.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return opt;
        }

        public static Task Main(string[] args) => CreateHostBuilder(args).Build().RunAsync();

        public static WebAssemblyHostBuilder CreateHostBuilder(string[] args) => WebAssemblyHostBuilder
            .CreateDefault(args)
            .AddRootComponent<App>("#app")
            .ConfigureServices((builder, services) =>
            {
                var assemblyName = Assembly.GetEntryAssembly()?.GetName();
                var userAgent = $"{assemblyName?.Name ?? "ERROR"} v{assemblyName?.Version?.ToString() ?? "ERROR"}";

                services.Configure<BackendOptions>(builder.Configuration.GetSection("Backend"));

                services.AddScoped(_ => new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
                    DefaultRequestHeaders = { { "User-Agent", userAgent } }
                });
                services.AddHttpClient("InternalReports", (sp, client) =>
                {
                    client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}reports/");
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                }).AddHttpMessageHandler<AssetsDelegatingHandler>();
                services.AddHttpClient("CrashReporter", (sp, client) =>
                {
                    var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                    client.BaseAddress = new Uri($"{backendOptions.Endpoint}/Reports/");
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                });
                services.AddHttpClient("Backend").ConfigureHttpClient((sp, client) =>
                {
                    var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                    client.BaseAddress = new Uri(backendOptions.Endpoint);
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

                services.Configure<JsonSerializerOptions>(opt => Configure(opt));

                services.AddTransient<AssetsDelegatingHandler>();
                services.AddTransient<AuthenticationDelegatingHandler>();

                services.AddScoped<DefaultBackendProvider>();
                services.AddScoped<IAuthenticationProvider, DefaultAuthenticationProvider>();
                //services.AddScoped<IAuthenticationProvider, DefaultBackendProvider>(sp => sp.GetRequiredService<DefaultBackendProvider>());
                services.AddScoped<IProfileProvider, DefaultBackendProvider>(sp => sp.GetRequiredService<DefaultBackendProvider>());
                services.AddScoped<IRoleProvider, DefaultBackendProvider>(sp => sp.GetRequiredService<DefaultBackendProvider>());
                services.AddScoped<IModProvider, DefaultBackendProvider>(sp => sp.GetRequiredService<DefaultBackendProvider>());
                services.AddScoped<ICrashReportProvider, DefaultBackendProvider>(sp => sp.GetRequiredService<DefaultBackendProvider>());

                services.AddScoped<ITokenContainer, LocalStorageTokenContainer>();
                services.AddScoped<StorageCache>();

                services.AddTransient<BrotliDecompressorService>();

                services.AddBlazoredLocalStorage();
                services.AddBlazoredSessionStorage();

                services.AddAuthorizationCore();
                services.AddScoped<SimpleAuthenticationStateProvider>();
                services.AddScoped<AuthenticationStateProvider, SimpleAuthenticationStateProvider>(sp => sp.GetRequiredService<SimpleAuthenticationStateProvider>());

                services.AddBlazorise(options =>
                {
                    options.ChangeTextOnKeyPress = true;
                });
                services.AddBootstrap5Providers();
                services.AddFontAwesomeIcons();

                services.Replace(ServiceDescriptor.Scoped<ITextLocalizerService, InvariantTextLocalizerService>());
            });
    }
}