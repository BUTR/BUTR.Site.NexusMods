using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BUTR.Site.NexusMods.Server.Utils.Http.StreamingJson;

public static class StreamingJsonDependencyInjectionExtensions
{
    public static IServiceCollection AddMultipartSupport(this IServiceCollection services)
    {
        return services.AddSingleton<IActionResultExecutor<StreamingJsonActionResult>>(new StreamingJsonActionResultExecutor());
    }
}