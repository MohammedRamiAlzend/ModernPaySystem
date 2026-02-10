using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core.Enums;
using ExpressionBuilderLib.src.Core.Interfaces;
using ExpressionBuilderLib.src.Utilities;

namespace ExpressionBuilderLib.src.Core;

/// <summary>
/// Combines multiple expressions using logical operators
/// </summary>
public static class ExpressionCombiner
{
    /// <summary>
    /// Combines two expressions with the specified logical operator
    /// </summary>
    public static Expression<Func<T, bool>> Combine<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2,
        LogicalOperator logicalOperator)
    {
        if (expr1 == null) return expr2;
        if (expr2 == null) return expr1;

        var parameter = Expression.Parameter(typeof(T));

        var left = ParameterReplacer.Replace(expr1, parameter);
        var right = ParameterReplacer.Replace(expr2, parameter);

        BinaryExpression combined = logicalOperator switch
        {
            LogicalOperator.And => Expression.AndAlso(left, right),
            LogicalOperator.Or => Expression.OrElse(left, right),
            LogicalOperator.AndNot => Expression.AndAlso(left, Expression.Not(right)),
            LogicalOperator.OrNot => Expression.OrElse(left, Expression.Not(right)),
            _ => Expression.AndAlso(left, right)
        };

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    /// <summary>
    /// Combines multiple expressions with AND operator
    /// </summary>
    public static Expression<Func<T, bool>> AndAll<T>(
        params Expression<Func<T, bool>>[] expressions)
    {
        if (expressions == null || expressions.Length == 0)
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), Expression.Parameter(typeof(T)));

        if (expressions.Length == 1)
            return expressions[0];

        var result = expressions[0];
        for (int i = 1; i < expressions.Length; i++)
        {
            result = Combine(result, expressions[i], LogicalOperator.And);
        }

        return result;
    }

    /// <summary>
    /// Combines multiple expressions with OR operator
    /// </summary>
    public static Expression<Func<T, bool>> OrAll<T>(
        params Expression<Func<T, bool>>[] expressions)
    {
        if (expressions == null || expressions.Length == 0)
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), Expression.Parameter(typeof(T)));

        if (expressions.Length == 1)
            return expressions[0];

        var result = expressions[0];
        for (int i = 1; i < expressions.Length; i++)
        {
            result = Combine(result, expressions[i], LogicalOperator.Or);
        }

        return result;
    }

    /// <summary>
    /// Negates an expression
    /// </summary>
    public static Expression<Func<T, bool>> Negate<T>(Expression<Func<T, bool>> expression)
    {
        if (expression == null)
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(false), Expression.Parameter(typeof(T)));

        var notExpr = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(notExpr, expression.Parameters);
    }
}
