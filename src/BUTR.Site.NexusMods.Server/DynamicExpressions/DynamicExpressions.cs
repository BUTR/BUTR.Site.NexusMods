using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BUTR.Site.NexusMods.Server.DynamicExpressions;

/// <summary>
/// Static class for creating dynamic expressions.
/// </summary>
public static class DynamicExpressions
{
    // Method references for various operations
    private static readonly MethodInfo _toStringMethod = typeof(object).GetMethod("ToString")!;

    private static readonly MethodInfo _stringContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
    private static readonly MethodInfo _stringContainsMethodIgnoreCase = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) })!;
    private static readonly MethodInfo _enumerableContainsMethod = typeof(Enumerable).GetMethods().Where(x => string.Equals(x.Name, "Contains", StringComparison.OrdinalIgnoreCase)).Single(x => x.GetParameters().Length == 2).MakeGenericMethod(typeof(string));
    private static readonly MethodInfo _dictionaryContainsKeyMethod = typeof(Dictionary<string, string>).GetMethods().Single(x => string.Equals(x.Name, "ContainsKey", StringComparison.OrdinalIgnoreCase));
    private static readonly MethodInfo _dictionaryContainsValueMethod = typeof(Dictionary<string, string>).GetMethods().Single(x => string.Equals(x.Name, "ContainsValue", StringComparison.OrdinalIgnoreCase));

    private static readonly MethodInfo _endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;
    private static readonly MethodInfo _isNullOrEmtpyMethod = typeof(string).GetMethod("IsNullOrEmpty", new[] { typeof(string) })!;
    private static readonly MethodInfo _startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;

    /// <summary>
    /// Creates a predicate expression for filtering entities based on a property, operator, and value.
    /// </summary>
    public static Expression<Func<TEntity, bool>> GetPredicate<TEntity>(string property, FilterOperator op, object value)
    {
        var param = Expression.Parameter(typeof(TEntity));
        return Expression.Lambda<Func<TEntity, bool>>(GetFilter(param, property, op, value), param);
    }

    /// <summary>
    /// Creates a property getter expression for a given property.
    /// </summary>
    public static Expression<Func<TEntity, object>> GetPropertyGetter<TEntity>(string property)
    {
        ArgumentNullException.ThrowIfNull(property);

        var param = Expression.Parameter(typeof(TEntity));
        var prop = param.GetNestedProperty(property);
        var convertedProp = Expression.Convert(prop, typeof(object));
        return Expression.Lambda<Func<TEntity, object>>(convertedProp, param);
    }

    internal static Expression GetFilter(ParameterExpression param, string property, FilterOperator op, object value)
    {
        var constant = Expression.Constant(value);
        var prop = param.GetNestedProperty(property);
        return CreateFilter(prop, op, constant);
    }

    private static Expression CreateFilter(MemberExpression prop, FilterOperator op, ConstantExpression constant) => op switch
    {
        FilterOperator.Equals => RobustEquals(prop, constant),
        FilterOperator.GreaterThan => Expression.GreaterThan(prop, constant),
        FilterOperator.LessThan => Expression.LessThan(prop, constant),
        FilterOperator.ContainsIgnoreCase => Expression.Call(AsString(prop), _stringContainsMethodIgnoreCase, AsString(constant), Expression.Constant(StringComparison.OrdinalIgnoreCase)),
        FilterOperator.Contains => GetContainsMethodCallExpression(prop, constant),
        FilterOperator.NotContains => Expression.Not(GetContainsMethodCallExpression(prop, constant)),
        FilterOperator.ContainsKey => Expression.Call(prop, _dictionaryContainsKeyMethod, AsString(constant)),
        FilterOperator.NotContainsKey => Expression.Not(Expression.Call(prop, _dictionaryContainsKeyMethod, AsString(constant))),
        FilterOperator.ContainsValue => Expression.Call(prop, _dictionaryContainsValueMethod, AsString(constant)),
        FilterOperator.NotContainsValue => Expression.Not(Expression.Call(prop, _dictionaryContainsValueMethod, AsString(constant))),
        FilterOperator.StartsWith => Expression.Call(AsString(prop), _startsWithMethod, AsString(constant)),
        FilterOperator.EndsWith => Expression.Call(AsString(prop), _endsWithMethod, AsString(constant)),
        FilterOperator.DoesntEqual => Expression.Not(RobustEquals(prop, constant)),
        FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(prop, constant),
        FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(prop, constant),
        FilterOperator.IsEmpty => Expression.Call(_isNullOrEmtpyMethod, AsString(prop)),
        FilterOperator.IsNotEmpty => Expression.Not(Expression.Call(_isNullOrEmtpyMethod, AsString(prop))),
        _ => throw new NotImplementedException()
    };

    private static Expression RobustEquals(MemberExpression prop, ConstantExpression constant)
    {
        if (prop.Type == typeof(bool) && bool.TryParse(constant.Value?.ToString(), out var val))
        {
            return Expression.Equal(prop, Expression.Constant(val));
        }

        if (constant.Value is { } value && TypeDescriptor.GetConverter(prop.Type) is { } typeConverter && typeConverter.CanConvertFrom(value.GetType()))
        {
            return Expression.Equal(prop, Expression.Convert(constant, prop.Type));
        }

        return Expression.Equal(prop, constant);
    }

    private static Expression GetContainsMethodCallExpression(MemberExpression prop, ConstantExpression constant)
    {
        var type = Nullable.GetUnderlyingType(constant.Type) ?? constant.Type;

        if (type == typeof(string)) // Check if it's convertible to string
            return Expression.Call(prop, _stringContainsMethod, AsString(constant));
        if (TypeDescriptor.GetConverter(type) is { } typeConverter && typeConverter.CanConvertTo(typeof(string)))
            return Expression.Call(AsString(prop), _stringContainsMethod, AsString(constant));
        if (type.GetInterfaces().Contains(typeof(IDictionary)))
            return Expression.Or(Expression.Call(prop, _dictionaryContainsKeyMethod, AsString(constant)), Expression.Call(prop, _dictionaryContainsValueMethod, AsString(constant)));
        if (type.GetInterfaces().Contains(typeof(IEnumerable)))
            return Expression.Call(_enumerableContainsMethod, prop, AsString(constant));

        throw new NotImplementedException($"{type} contains is not implemented.");
    }

    private static Expression AsString(Expression constant)
    {
        var type = Nullable.GetUnderlyingType(constant.Type) ?? constant.Type;

        if (type == typeof(string))
            return constant;

        if (TypeDescriptor.GetConverter(type) is { } typeConverter && typeConverter.CanConvertTo(typeof(string)))
            return Expression.Convert(constant, typeof(string));

        var convertedExpr = Expression.Convert(constant, typeof(object));
        return Expression.Call(convertedExpr, _toStringMethod);
    }
}