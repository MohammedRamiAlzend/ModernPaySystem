using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace ExpressionBuilderLib.src.Utilities;


/// <summary>
/// Replaces parameters in expression trees
/// </summary>
public class ParameterReplacer : ExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> _parameterMap;

    public ParameterReplacer(Dictionary<ParameterExpression, ParameterExpression> parameterMap)
    {
        _parameterMap = parameterMap ?? new Dictionary<ParameterExpression, ParameterExpression>();
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (_parameterMap.TryGetValue(node, out var replacement))
        {
            return replacement;
        }
        return base.VisitParameter(node);
    }

    public static Expression Replace<T>(Expression<Func<T, bool>> expression, ParameterExpression newParameter)
    {
        var parameterMap = new Dictionary<ParameterExpression, ParameterExpression>
        {
            [expression.Parameters[0]] = newParameter
        };

        var replacer = new ParameterReplacer(parameterMap);
        return replacer.Visit(expression.Body);
    }
}
