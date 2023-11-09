using System.Linq.Expressions;

namespace BUTR.Site.NexusMods.Server.DynamicExpressions;

public static class ExpressionExtensions
{
    internal static MemberExpression GetNestedProperty(this Expression param, string property)
    {
        var propNames = property.Split('.');
        var propExpr = Expression.Property(param, propNames[0]);

        for (var i = 1; i < propNames.Length; i++)
        {
            propExpr = Expression.Property(propExpr, propNames[i]);
        }

        return propExpr;
    }
}