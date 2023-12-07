using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public static class ApiResultUtils
{
    public static bool IsReturnTypeApiResult(MethodInfo methodInfo)
    {
        var returnType = methodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() is { } typeDef && (typeDef == typeof(Task<>) || typeDef == typeof(ValueTask<>)))
            returnType = returnType.GenericTypeArguments[0];

        return returnType.GetInterfaces().Any(x => x == typeof(IApiResult));
    }
}