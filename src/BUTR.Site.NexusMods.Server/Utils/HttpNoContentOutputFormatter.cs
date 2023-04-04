using Microsoft.AspNetCore.Mvc.Formatters;

using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils
{
    /// <summary>
    /// Sets the status code to 200 if the content is null.
    /// </summary>
    public sealed class HttpNoContentOutputFormatter2 : IOutputFormatter
    {
        /// <summary>
        /// Indicates whether to select this formatter if the returned value from the action
        /// is null.
        /// </summary>
        public bool TreatNullValueAsNoContent { get; set; } = true;

        /// <inheritdoc />
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            // ignore the contentType and just look at the content.
            // This formatter will be selected if the content is null.
            // We check for Task as a user can directly create an ObjectContentResult with the unwrapped type.
            if (context.ObjectType == typeof(void) || context.ObjectType == typeof(Task))
            {
                return true;
            }

            return TreatNullValueAsNoContent && context.Object == null;
        }

        /// <inheritdoc />
        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            return Task.CompletedTask;
        }
    }
}