using Blazorise.Localization;

using BUTR.Site.NexusMods.Client.Extensions;
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

using Config = Blazorise.Config;
using ServiceCollectionExtensions = Blazored.LocalStorage.ServiceCollectionExtensions;

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
                OptionsConfigurationServiceCollectionExtensions.Configure<BackendOptions>(services, builder.Configuration.GetSection("Backend"));

                var assemblyName = Assembly.GetEntryAssembly()?.GetName();
                var userAgent = $"{assemblyName?.Name} v{assemblyName?.Version}";
                ServiceCollectionServiceExtensions.AddScoped(services, _ => new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
                    DefaultRequestHeaders = { { "User-Agent", userAgent } }
                });
                HttpClientFactoryServiceCollectionExtensions.AddHttpClient(services, "InternalReports", (sp, client) =>
                {
                    client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}reports/");
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                });
                HttpClientFactoryServiceCollectionExtensions.AddHttpClient(services, "Backend").ConfigureHttpClient((sp, client) =>
                {
                    var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
                    client.BaseAddress = new Uri(backendOptions.Endpoint);
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
                HttpClientFactoryServiceCollectionExtensions.AddHttpClient(services, "CrashReporter", (sp, client) =>
                {
                    var backendOptions = ServiceProviderServiceExtensions.GetRequiredService<IOptions<BackendOptions>>(sp).Value;
                    client.BaseAddress = new Uri($"{backendOptions.Endpoint}/Reports/");
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                });

                OptionsServiceCollectionExtensions.Configure<JsonSerializerOptions>(services, opt => Configure(opt));

                ServiceCollectionServiceExtensions.AddTransient<AuthenticationDelegatingHandler>(services);

                ServiceCollectionServiceExtensions.AddScoped<DefaultBackendProvider>(services);
                ServiceCollectionServiceExtensions.AddScoped<IAuthenticationProvider, DefaultAuthenticationProvider>(services);
                //services.AddScoped<IAuthenticationProvider, DefaultBackendProvider>(sp => sp.GetRequiredService<DefaultBackendProvider>());
                ServiceCollectionServiceExtensions.AddScoped<IProfileProvider, DefaultBackendProvider>(services, sp => ServiceProviderServiceExtensions.GetRequiredService<DefaultBackendProvider>(sp));
                ServiceCollectionServiceExtensions.AddScoped<IRoleProvider, DefaultBackendProvider>(services, sp => ServiceProviderServiceExtensions.GetRequiredService<DefaultBackendProvider>(sp));
                ServiceCollectionServiceExtensions.AddScoped<IModProvider, DefaultBackendProvider>(services, sp => ServiceProviderServiceExtensions.GetRequiredService<DefaultBackendProvider>(sp));
                ServiceCollectionServiceExtensions.AddScoped<ICrashReportProvider, DefaultBackendProvider>(services, sp => ServiceProviderServiceExtensions.GetRequiredService<DefaultBackendProvider>(sp));

                ServiceCollectionServiceExtensions.AddScoped<ITokenContainer, LocalStorageTokenContainer>(services);
                ServiceCollectionServiceExtensions.AddScoped<LocalStorageCache>(services);

                ServiceCollectionExtensions.AddBlazoredLocalStorage(services);
                Blazored.SessionStorage.ServiceCollectionExtensions.AddBlazoredSessionStorage(services);

                AuthorizationServiceCollectionExtensions.AddAuthorizationCore(services);
                ServiceCollectionServiceExtensions.AddScoped<SimpleAuthenticationStateProvider>(services);
                ServiceCollectionServiceExtensions.AddScoped<AuthenticationStateProvider, SimpleAuthenticationStateProvider>(services, sp => ServiceProviderServiceExtensions.GetRequiredService<SimpleAuthenticationStateProvider>(sp));

                Config.AddBlazorise(services, options =>
                {
                    options.ChangeTextOnKeyPress = true;
                });
                Blazorise.Bootstrap5.Config.AddBootstrap5Providers(services);
                Blazorise.Icons.FontAwesome.Config.AddFontAwesomeIcons(services);
                ServiceCollectionDescriptorExtensions.Replace(services, ServiceDescriptor.Scoped<ITextLocalizerService, InvariantTextLocalizerService>());

            });
    }
}