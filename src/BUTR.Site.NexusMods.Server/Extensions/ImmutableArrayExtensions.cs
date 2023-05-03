using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class ImmutableArrayExtensions
{
    public static T[] AsArray<T>(this ImmutableArray<T> immutableArray) => Unsafe.As<ImmutableArray<T>, T[]>(ref immutableArray);
    public static ImmutableArray<T> AsImmutableArray<T>(this T[] array) => Unsafe.As<T[], ImmutableArray<T>>(ref array);
}