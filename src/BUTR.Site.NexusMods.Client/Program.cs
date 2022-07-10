using Blazored.LocalStorage;
using Blazored.SessionStorage;

using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise.Localization;

using BUTR.Site.NexusMods.Client.Extensions;
using BUTR.Site.NexusMods.Client.Options;
using BUTR.Site.NexusMods.Client.Services;
using BUTR.Site.NexusMods.ServerClient;

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
        private static JsonSerializerOptions Configure(this JsonSerializerOptions opt)
        {
            opt.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.PropertyNameCaseInsensitive = true;
            opt.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            opt.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return opt;
        }

        private static TClient ConfigureClient<TClient>(IServiceProvider sp, Func<HttpClient, JsonSerializerOptions, TClient> factory, string backend = "Backend")
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(backend);
            var opt = sp.GetRequiredService<IOptions<JsonSerializerOptions>>();

            return factory(httpClient, opt.Value);
        }

        private static IHttpClientBuilder ConfigureBackend(this IHttpClientBuilder builder, string userAgent) => builder.ConfigureHttpClient((sp, client) =>
        {
            var backendOptions = sp.GetRequiredService<IOptions<BackendOptions>>().Value;
            client.BaseAddress = new Uri(backendOptions.Endpoint);
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        });


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
                services.AddHttpClient("InternalReports").ConfigureHttpClient((_, client) =>
                {
                    client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}reports/");
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                }).AddHttpMessageHandler<AssetsDelegatingHandler>();
                services.AddHttpClient("BackendAuthentication").ConfigureBackend(userAgent)
                    .AddHttpMessageHandler<AuthenticationInjectionDelegatingHandler>();
                services.AddHttpClient("Backend").ConfigureBackend(userAgent)
                    .AddHttpMessageHandler<AuthenticationAnd401DelegatingHandler>();

                services.Configure<JsonSerializerOptions>(opt => opt.Configure());

                services.AddTransient<AssetsDelegatingHandler>();
                services.AddTransient<AuthenticationInjectionDelegatingHandler>();
                services.AddTransient<AuthenticationAnd401DelegatingHandler>();

                services.AddTransient<IAuthenticationClient, AuthenticationClient>(sp => ConfigureClient(sp, (http, opt) => new AuthenticationClient(http, opt), "BackendAuthentication"));
                services.AddTransient<IUserClient, UserClient>(sp => ConfigureClient(sp, (http, opt) => new UserClient(http, opt)));
                services.AddTransient<ICrashReportsClient, CrashReportsClient>(sp => ConfigureClient(sp, (http, opt) => new CrashReportsClient(http, opt)));
                services.AddTransient<IModClient, ModClient>(sp => ConfigureClient(sp, (http, opt) => new ModClient(http, opt)));
                services.AddTransient<IReportsClient, ReportsClient>(sp => ConfigureClient(sp, (http, opt) => new ReportsClient(http, opt)));
                services.AddTransient<IUserClient, UserClient>(sp => ConfigureClient(sp, (http, opt) => new UserClient(http, opt)));

                services.AddScoped<IAuthenticationProvider, DefaultAuthenticationProvider>();
                services.AddScoped<IProfileProvider, DefaultProfileProvider>();
                services.AddScoped<IRoleProvider, DefaultRoleProvider>();
                services.AddScoped<IModProvider, DefaultModProvider>();
                services.AddScoped<ICrashReportProvider, DefaultCrashReportProvider>();

                services.AddScoped<ITokenContainer, LocalStorageTokenContainer>();

                services.AddScoped<StorageCache>();

                services.AddTransient<BrotliDecompressorService>();
                services.AddTransient<DownloadFileService>();

                services.AddBlazoredLocalStorage();
                services.AddBlazoredSessionStorage();

                services.AddAuthorizationCore();
                services.AddScoped<AuthenticationStateProvider, SimpleAuthenticationStateProvider>();

                services.AddBlazorise(options =>
                {
                    options.Immediate = true;
                });
                services.AddBootstrap5Providers();
                services.AddFontAwesomeIcons();
                services.Replace(ServiceDescriptor.Scoped<ITextLocalizerService, InvariantTextLocalizerService>());
            });
    }
}