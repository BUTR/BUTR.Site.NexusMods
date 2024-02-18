using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace BUTR.Site.NexusMods.Server.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ImmutableArray{T}"/> objects.
/// </summary>
public static class ImmutableArrayExtensions
{
    /// <summary>
    /// Converts an <see cref="ImmutableArray{T}"/> to a regular array.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="immutableArray">The immutable array to convert.</param>
    /// <returns>A regular array containing the same elements as the immutable array.</returns>
    public static T[] AsArray<T>(this ImmutableArray<T> immutableArray) => Unsafe.As<ImmutableArray<T>, T[]>(ref immutableArray);
    
    /// <summary>
    /// Converts a regular array to an <see cref="ImmutableArray{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The regular array to convert.</param>
    /// <returns>An immutable array containing the same elements as the regular array.</returns>
    public static ImmutableArray<T> AsImmutableArray<T>(this T[] array) => Unsafe.As<T[], ImmutableArray<T>>(ref array);
}