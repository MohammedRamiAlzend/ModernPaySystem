using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ExpressionBuilderLib.src.Core;

namespace ExpressionBuilderLib.src.Extensions;

/// <summary>
/// Extension methods for expressions.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Combines two expressions with AND.
    /// </summary>
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        return ExpressionCombiner.Combine(expr1, expr2, Core.Enums.LogicalOperator.And);
    }

    /// <summary>
    /// Combines two expressions with OR.
    /// </summary>
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        return ExpressionCombiner.Combine(expr1, expr2, Core.Enums.LogicalOperator.Or);
    }

    /// <summary>
    /// Negates an expression
    /// </summary>
    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
    {
        return ExpressionCombiner.Negate(expression);
    }

    /// <summary>
    /// Gets the property name from a property expression
    /// </summary>
    public static string GetPropertyName<T, TProperty>(
        this Expression<Func<T, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        throw new ArgumentException("Expression is not a property access", nameof(propertyExpression));
    }
}
