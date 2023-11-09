using System.Reflection;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.APIResponses;

public static class APIResponseUtils
{
    public static bool IsReturnTypeAPIResponse(MethodInfo methodInfo)
    {
        var returnType = methodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() is { } typeDef && (typeDef == typeof(Task<>) || typeDef == typeof(ValueTask<>)))
            returnType = returnType.GenericTypeArguments[0];

        return returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(APIResponseActionResult<>);
    }
}