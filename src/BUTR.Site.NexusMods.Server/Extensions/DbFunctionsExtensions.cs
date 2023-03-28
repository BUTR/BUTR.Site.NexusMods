using System;
using System.Collections.Generic;

namespace BUTR.Site.NexusMods.Server.Extensions;

public static class DbFunctionsExtensions
{
    public static bool HasKeyValue(Dictionary<string, string> dic, string key, string @operator, string value) =>
        throw new NotImplementedException("For use only as an EF core Db function");
    public static bool HasKeysValues(Dictionary<string, string> dic, string[] keys, string @operator, string[] values) =>
        throw new NotImplementedException("For use only as an EF core Db function");
}