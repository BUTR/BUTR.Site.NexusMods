using System.IO;
using System.Text;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="TextWriter"/> objects.
/// </summary>
public static class TextWriterExtensions
{
    /// <summary>
    /// Writes a portion of a <see cref="StringBuilder"/> to a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
    /// <param name="sb">The <see cref="StringBuilder"/> to write from.</param>
    /// <param name="offset">The zero-based index in the <see cref="StringBuilder"/> at which to start writing.</param>
    /// <param name="length">The number of characters to write.</param>
    public static void Write(this TextWriter writer, StringBuilder sb, int offset, int length)
    {
        var buffer = length > 512 ? new char[length] : stackalloc char[length];
        sb.CopyTo(offset, buffer, length);
        writer.Write(buffer);
    }
}