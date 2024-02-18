using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public static class ApiResultUtils
{
    public static bool IsReturnTypeApiResult(MethodInfo? methodInfo)
    {
        if (methodInfo is null) return false;
        
        var returnType = GetReturnType(methodInfo);

        return returnType == typeof(ApiResult) || returnType.GetGenericTypeDefinition() == typeof(ApiResult<>);
    }

    private static Type GetReturnType(MethodInfo methodInfo)
    {
        var returnType = methodInfo.ReturnType;
        if (returnType.IsGenericType && IsTaskType(returnType.GetGenericTypeDefinition()))
            returnType = returnType.GenericTypeArguments[0];

        return returnType;
    }

    private static bool IsTaskType(Type type)
    {
        return type == typeof(Task<>) || type == typeof(ValueTask<>);
    }
}