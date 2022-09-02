using SharpCompress.Readers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace BUTR.Site.NexusMods.Server.Extensions
{
    public static class ReaderExtensions
    {
        public static IReader? OpenOrDefault(Stream stream, ReaderOptions options)
        {
            TryOpen(stream, options, out var archive);
            return archive;
        }

        public static bool TryOpen(Stream stream, ReaderOptions options, [NotNullWhen(true)] out IReader? archive)
        {
            try
            {
                archive = ReaderFactory.Open(stream, options);
                return true;
            }
            catch (Exception)
            {
                archive = null;
                return false;
            }
        }
    }
}