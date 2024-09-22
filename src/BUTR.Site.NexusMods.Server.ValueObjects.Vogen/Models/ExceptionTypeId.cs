namespace BUTR.Site.NexusMods.Server.Models;

using TType = ExceptionTypeId;
using TValueType = String;

[ValueObject<TValueType>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson | Conversions.TypeConverter)]
public readonly partial struct ExceptionTypeId
{
    public static bool TryParseFromException(TValueType exception, out TType value)
    {
        Span<Range> dest = stackalloc Range[32];
        ReadOnlySpan<char> lastTypeLine = default;
        foreach (ReadOnlySpan<char> line in exception.SplitLines())
        {
            var count = line.Split(dest, ':');
            if (count != 2) continue;
            var firstPart = line[dest[0]].Trim();
            var secondPart = line[dest[1]].Trim();
            if (firstPart is "Type")
                lastTypeLine = secondPart;
        }

        if (lastTypeLine.Length > 0)
        {
            value = From(lastTypeLine.ToString());
            return true;
        }

        value = From("");
        return false;
    }
}