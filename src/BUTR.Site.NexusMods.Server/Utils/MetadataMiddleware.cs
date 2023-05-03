using BUTR.Site.NexusMods.Server.Extensions;

using Microsoft.AspNetCore.Http;

using System;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils;

public class MetadataMiddleware
{
    private readonly RequestDelegate _next;

    public MetadataMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task Invoke(HttpContext context)
    {
        Task Unauthorized()
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var endpoint = context.GetEndpoint();

        var metadataData = endpoint?.Metadata.GetOrderedMetadata<IMetadataData>();
        if (metadataData is null)
        {
            await _next(context);
            return;
        }

        if (context.GetMetadata() is not { } userMetadata)
        {
            await Unauthorized();
            return;
        }

        foreach (var metadata in metadataData)
        {
            if (!userMetadata.TryGetValue(metadata.Key, out var userValue))
            {
                await Unauthorized();
                return;
            }
            if (metadata.Value is { } value && value != userValue)
            {
                await Unauthorized();
                return;
            }
        }

        await _next(context);
    }
}