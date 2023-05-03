using Blazorise;

using BUTR.Site.NexusMods.ServerClient;

using System;
using System.Text;

namespace BUTR.Site.NexusMods.Client.Extensions;

public static class EnumExtension
{
    public static SortingType ToSortingType(this SortDirection sortDirection) => sortDirection switch
    {
        SortDirection.Ascending => SortingType.Ascending,
        SortDirection.Descending => SortingType.Descending,
        _ => SortingType.Descending
    };

    public static string GetDisplayName<T>(this T @enum) where T : Enum
    {
        var sb = new StringBuilder();

        var previousChar = char.MinValue; // Unicode '\0'

        foreach (var c in @enum.ToString())
        {
            if (char.IsUpper(c))
            {
                // If not the first character and previous character is not a space, insert a space before uppercase

                if (sb.Length != 0 && previousChar != ' ')
                {
                    sb.Append(' ');
                }
            }

            sb.Append(c);

            previousChar = c;
        }

        return sb.ToString();
    }
}