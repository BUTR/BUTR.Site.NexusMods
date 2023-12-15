using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using System;

namespace BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

public sealed class ApiResultActionResultTypeMapper : IActionResultTypeMapper
{
    private readonly IActionResultTypeMapper _implementation;

    public ApiResultActionResultTypeMapper(IActionResultTypeMapper implementation)
    {
        _implementation = implementation;
    }

    public Type GetResultDataType(Type returnType)
    {
        if (returnType == typeof(ApiResult))
            return typeof(ApiResultModel);
        
        if (returnType.IsGenericType && IsSubclassOfGeneric(returnType, typeof(ApiResult<>)))
            return typeof(ApiResultModel<>).MakeGenericType(returnType.GenericTypeArguments[0]);
        
        return _implementation.GetResultDataType(returnType);
    }

    public IActionResult Convert(object? value, Type returnType) => _implementation.Convert(value, returnType);
    
    private static bool IsSubclassOfGeneric(Type current, Type genericBase)
    {
        do 
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericBase)
                return true;
        }
        while((current = current.BaseType!) != null);
        return false;
    }
}