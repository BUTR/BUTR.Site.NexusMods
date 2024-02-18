using System;
using System.Linq.Expressions;

namespace BUTR.Site.NexusMods.Server.DynamicExpressions;

/// <summary>
/// A builder class for creating dynamic filters.
/// </summary>
public class DynamicFilterBuilder<TEntity>
{
    /// <summary>
    /// The current expression being built.
    /// </summary>
    private Expression? Expression { get; set; }

    /// <summary>
    /// The parameter expression used in the filter.
    /// </summary>
    private readonly ParameterExpression _param;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicFilterBuilder{TEntity}"/> class.
    /// </summary>
    public DynamicFilterBuilder() : this(Expression.Parameter(typeof(TEntity))) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicFilterBuilder{TEntity}"/> class with a specific parameter expression.
    /// </summary>
    private DynamicFilterBuilder(ParameterExpression param) { _param = param; }

    /// <summary>
    /// Adds a new AND condition to the filter.
    /// </summary>
    public DynamicFilterBuilder<TEntity> And(string property, FilterOperator op, object value)
    {
        var newExpr = DynamicExpressions.GetFilter(_param, property, op, value);
        Expression = Expression == null ? newExpr : Expression.AndAlso(Expression, newExpr);
        return this;
    }

    /// <summary>
    /// Adds a new AND condition to the filter using a sub-builder.
    /// </summary>
    public DynamicFilterBuilder<TEntity> And(Action<DynamicFilterBuilder<TEntity>> action)
    {
        var builder = new DynamicFilterBuilder<TEntity>(_param);
        action(builder);

        if (builder.Expression == null)
            throw new Exception("Empty builder");

        Expression = Expression == null ? builder.Expression : Expression.AndAlso(Expression, builder.Expression);
        return this;
    }

    /// <summary>
    /// Adds a new OR condition to the filter.
    /// </summary>
    public DynamicFilterBuilder<TEntity> Or(string property, FilterOperator op, object value)
    {
        var newExpr = DynamicExpressions.GetFilter(_param, property, op, value);
        Expression = Expression == null ? newExpr : Expression.OrElse(Expression, newExpr);
        return this;
    }

    /// <summary>
    /// Adds a new OR condition to the filter using a sub-builder.
    /// </summary>
    public DynamicFilterBuilder<TEntity> Or(Action<DynamicFilterBuilder<TEntity>> action)
    {
        var builder = new DynamicFilterBuilder<TEntity>(_param);
        action(builder);

        if (builder.Expression == null)
            throw new Exception("Empty builder");

        Expression = Expression == null ? builder.Expression : Expression.OrElse(Expression, builder.Expression);
        return this;
    }

    /// <summary>
    /// Builds the filter into an expression.
    /// </summary>
    public Expression<Func<TEntity, bool>> Build() => Expression.Lambda<Func<TEntity, bool>>(Expression!, _param);

    /// <summary>
    /// Compiles the filter into a delegate.
    /// </summary>
    public Func<TEntity, bool> Compile() => Build().Compile();
}