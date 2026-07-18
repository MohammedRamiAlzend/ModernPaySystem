using ExpressionBuilderLib.src.Core.Enums;
using System.Linq.Expressions;

namespace ExpressionBuilderLib.src.Core.Interfaces;

public interface IPropertyFilter<T>
{
    string PropertyName { get; }
    object Value { get; }
    ComparisonOperator Operator { get; }
    StringComparisonMode StringComparison { get; }

    Expression<Func<T, bool>> BuildExpression();
}
