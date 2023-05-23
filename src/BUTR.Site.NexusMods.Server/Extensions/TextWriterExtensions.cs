using System.IO;
using System.Text;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class TextWriterExtensions
    {
        public static void Write(this TextWriter writer, StringBuilder sb, int offset, int length)
        {
            var toTake = length;
            var chunkOffset = 0;
            foreach (var chunk in sb.GetChunks())
            {
                if (chunkOffset + chunk.Length < offset) continue;
            
                var skip = offset < chunkOffset ? 0 : offset - chunkOffset;
                var availableInChunk = chunk.Length - skip;
                if (availableInChunk >= toTake) // Fast exit
                {
                    writer.Write(chunk.Span.Slice(skip, toTake));
                    break;
                }

                writer.Write(chunk.Span.Slice(skip, availableInChunk));
                toTake -= availableInChunk;

                chunkOffset += chunk.Length;
            }
        }
    }
}