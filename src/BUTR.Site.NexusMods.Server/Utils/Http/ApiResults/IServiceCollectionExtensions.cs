using BUTR.Site.NexusMods.Server.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using System;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public static class IServiceCollectionExtensions
{
    public static IMvcBuilder AddControllersWithAPIResult(this IServiceCollection services, Action<MvcOptions> configure)
    {
        services.AddSingleton<IProblemDetailsWriter, ApiResultProblemDetailsWriter>();
        services.AddProblemDetails();

        return services.AddControllers().HandleInvalidModelStateError().AddMvcOptions(configure);
    }
}