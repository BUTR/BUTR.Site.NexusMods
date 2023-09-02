using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BUTR.Site.NexusMods.Server.Contexts;

public class ShortNullableConverter : ValueConverter<ushort?, short?>
{
    public ShortNullableConverter() : base(v => v != null ? (short) (v + short.MaxValue)! : null , v => v != null ? (ushort) (v - short.MaxValue)! : null) { }
}