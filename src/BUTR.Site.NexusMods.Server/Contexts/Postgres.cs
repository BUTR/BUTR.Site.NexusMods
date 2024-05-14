using System;

namespace BUTR.Site.NexusMods.Server.Contexts;

public static class Postgres
{
    public static class Functions
    {
        public static decimal Log(decimal d, decimal x) => throw new InvalidOperationException("This method is not meant to be called directly.");
        public static decimal Log(decimal x) => throw new InvalidOperationException("This method is not meant to be called directly.");
    }
}