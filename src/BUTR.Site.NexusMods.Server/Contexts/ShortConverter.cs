using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BUTR.Site.NexusMods.Server.Contexts;

public class ShortConverter : ValueConverter<ushort, short>
{
    public ShortConverter() : base(v => (short) (v + short.MaxValue) , v => (ushort) (v - short.MaxValue)) { }
}