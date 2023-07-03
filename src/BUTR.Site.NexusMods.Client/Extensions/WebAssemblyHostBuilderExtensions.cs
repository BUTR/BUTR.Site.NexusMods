using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;

namespace BUTR.Site.NexusMods.Client.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddRootComponent<TComponent>(this WebAssemblyHostBuilder builder, string selector)
        where TComponent : IComponent
    {
        builder.RootComponents?.Add<TComponent>(selector);
        return builder;
    }

    public static WebAssemblyHostBuilder ConfigureServices(this WebAssemblyHostBuilder builder, Action<WebAssemblyHostBuilder, IServiceCollection> configureDelegate)
    {
        configureDelegate?.Invoke(builder, builder.Services);
        return builder;
    }

    public static WebAssemblyHostBuilder ConfigureLogging(this WebAssemblyHostBuilder builder, Action<ILoggingBuilder, IServiceCollection> configureDelegate)
    {
        configureDelegate?.Invoke(builder.Logging, builder.Services);
        return builder;
    }
}