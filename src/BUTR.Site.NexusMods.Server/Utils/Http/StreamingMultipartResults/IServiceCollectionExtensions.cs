using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BUTR.Site.NexusMods.Server.Utils.Http.StreamingMultipartResults;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddStreamingMultipartResult(this IServiceCollection services)
    {
        return services.AddSingleton<IActionResultExecutor<StreamingMultipartResult>>(new StreamingMultipartResultExecutor());
    }
}