using SharpCompress.Archives;
using SharpCompress.Readers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for working with archives.
/// </summary>
public static class ArchiveExtensions
{
    /// <summary>
    /// Opens an archive from a stream, or returns null if the archive cannot be opened.
    /// </summary>
    /// <param name="stream">The stream containing the archive data.</param>
    /// <param name="options">The options to use when reading the archive.</param>
    /// <returns>The opened archive, or null if the archive cannot be opened.</returns>
    public static IArchive? OpenOrDefault(Stream stream, ReaderOptions options)
    {
        TryOpen(stream, options, out var archive);
        return archive;
    }

    /// <summary>
    /// Attempts to open an archive from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the archive data.</param>
    /// <param name="options">The options to use when reading the archive.</param>
    /// <param name="archive">When this method returns, contains the opened archive if the archive was opened successfully, or null if the archive could not be opened.</param>
    /// <returns>true if the archive was opened successfully; otherwise, false.</returns>

    public static bool TryOpen(Stream stream, ReaderOptions options, [NotNullWhen(true)] out IArchive? archive)
    {
        try
        {
            archive = ArchiveFactory.Open(stream, options);
            return true;
        }
        catch (Exception)
        {
            archive = null;
            return false;
        }
    }
}