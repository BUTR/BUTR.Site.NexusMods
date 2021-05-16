using Blazored.LocalStorage;

using BUTR.CrashReportViewer.Shared.Helpers;

using Flurl;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

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

            const string azureEndpoint = "https://butr-crashreportviewer.azurewebsites.net";
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });
            builder.Services.AddHttpClient("NexusModsAPI", client =>
            {
                //client.BaseAddress = new Uri("https://thingproxy.freeboard.io/fetch/https://api.nexusmods.com/");
                client.BaseAddress = new Uri(Url.Combine($"{azureEndpoint}", "/NexusModsAPIProxy/"));
                //client.BaseAddress = new Uri(Url.Combine($"{builder.HostEnvironment.BaseAddress}", "/NexusModsAPIProxy/"));
            });
            builder.Services.AddHttpClient("Backend", client =>
            {
                client.BaseAddress = new Uri(azureEndpoint);
                //client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });

            builder.Services.AddScoped<NexusModsAPIClient>();


            builder.Services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync();
        }
    }
}
