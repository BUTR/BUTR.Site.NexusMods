using System.Linq.Expressions;

namespace BUTR.Site.NexusMods.Server.DynamicExpressions;

/// <summary>
/// Provides extension methods for <see cref="Expression"/> objects.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Gets a nested property from a parameter expression.
    /// </summary>
    /// <param name="param">The parameter expression.</param>
    /// <param name="property">The dot-separated path to the nested property.</param>
    /// <returns>A <see cref="MemberExpression"/> representing the nested property.</returns>
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