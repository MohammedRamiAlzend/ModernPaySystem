using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ExpressionBuilderLib.src.Core.Enums;

namespace ExpressionBuilderLib.src.Core.Interfaces;

public interface IExpressionBuilder<T>
{
    ExpressionBuilder<T> And(Expression<Func<T, bool>> predicate);
    ExpressionBuilder<T> Or(Expression<Func<T, bool>> predicate);
    ExpressionBuilder<T> AndNot(Expression<Func<T, bool>> predicate);
    ExpressionBuilder<T> OrNot(Expression<Func<T, bool>> predicate);
    ExpressionBuilder<T> AddCondition(Expression<Func<T, bool>> predicate, LogicalOperator logicalOperator);

    Expression<Func<T, bool>> Build();
    Func<T, bool> Compile();
    void Reset();

    ExpressionBuilder<T> Clone();
    string ToString();
}
