using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace BUTR.Site.NexusMods.Server.Utils.Npgsql;

public sealed class ImmutableArrayToArrayConverter<T> : ValueConverter<ImmutableArray<T>, T[]>, INpgsqlArrayConverter
{
    private static ImmutableArray<T> ToImmutableArray(T[] array) => Unsafe.As<T[], ImmutableArray<T>>(ref array);
    private static T[] ToArray(ImmutableArray<T> immutableArray) => Unsafe.As<ImmutableArray<T>, T[]>(ref immutableArray);

    public ValueConverter ElementConverter => new ValueConverter<T, T>(x => x, x => x);

    public ImmutableArrayToArrayConverter() : base(x => ToArray(x), x => ToImmutableArray(x)) { }
}