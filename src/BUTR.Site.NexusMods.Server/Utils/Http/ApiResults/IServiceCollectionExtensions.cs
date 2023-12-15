using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;
using System.Linq;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public static class IServiceCollectionExtensions
{
    public static IMvcBuilder AddControllersWithAPIResult(this IServiceCollection services, Action<MvcOptions> configure)
    {
        services.AddSingleton<IProblemDetailsWriter, ApiResultProblemDetailsWriter>();
        services.AddProblemDetails();

        var builder = services.AddControllers().HandleInvalidModelStateError().AddMvcOptions(configure);
        
        services.Decorate<IActionResultTypeMapper, ApiResultActionResultTypeMapper>();

        return builder;
    }
    
    private static void Decorate<TInterface, TDecorator>(this IServiceCollection services)
        where TInterface : notnull
        where TDecorator : TInterface
    {
        if (services.SingleOrDefault(s => s.ServiceType == typeof(TInterface)) is not { } interfaceDescriptor)
            throw new InvalidOperationException($"{typeof(TInterface).Name} is not registered in injection container");

        if (ActivatorUtilities.CreateFactory(typeof(TDecorator), new[] { typeof(TInterface) }) is not { } decoratorFactory)
            throw new InvalidOperationException($"Cannot create factory for {typeof(TDecorator).Name}");

        services.Replace(ServiceDescriptor.Describe(
            typeof(TInterface),
            serviceProvider => (TInterface) decoratorFactory(serviceProvider, new[] { serviceProvider.CreateInstance(interfaceDescriptor) }),
            interfaceDescriptor.Lifetime));
    }

    private static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null)
            return descriptor.ImplementationInstance;

        if (descriptor.ImplementationFactory != null)
            return descriptor.ImplementationFactory(services);

        if (descriptor.ImplementationType == null)
            throw new InvalidOperationException($"Invalid service descriptor implementation type");

        if (ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType!) is not { } instance)
            throw new InvalidOperationException($"Cannot create instance of {descriptor.ImplementationType.Name}");

        return instance;
    }
}