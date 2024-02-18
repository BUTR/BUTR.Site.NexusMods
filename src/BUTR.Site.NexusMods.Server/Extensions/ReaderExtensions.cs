using SharpCompress.Readers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IReader"/> objects.
/// </summary>
public static class ReaderExtensions
{
    /// <summary>
    /// Opens a reader for the specified stream, or returns null if the reader cannot be opened.
    /// </summary>
    /// <param name="stream">The stream to open a reader for.</param>
    /// <param name="options">The options to use when opening the reader.</param>
    /// <returns>An <see cref="IReader"/> for the specified stream, or null if the reader cannot be opened.</returns>
    public static IReader? OpenOrDefault(Stream stream, ReaderOptions options)
    {
        TryOpen(stream, options, out var archive);
        return archive;
    }

    /// <summary>
    /// Tries to open a reader for the specified stream.
    /// </summary>
    /// <param name="stream">The stream to open a reader for.</param>
    /// <param name="options">The options to use when opening the reader.</param>
    /// <param name="archive">When this method returns, contains the opened reader, or null if the reader could not be opened.</param>
    /// <returns>true if a reader was successfully opened; otherwise, false.</returns>
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