using BUTR.Site.NexusMods.Server.Utils;

using Microsoft.AspNetCore.Builder;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class MetadataMiddlewareExtensions
    {
        public static IApplicationBuilder UseMetadata(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetadataMiddleware>();
        }
    }
}