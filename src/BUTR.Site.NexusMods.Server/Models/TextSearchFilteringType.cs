using System;

namespace BUTR.Site.NexusMods.Server.Models
{
    [Flags]
    public enum TextSearchFilteringType
    {
        None = 0,
        And = 1,
        Or = 2,
        Not = 4,
        AndNot = And | Not,
        OrNot = Or | Not,
    }
}