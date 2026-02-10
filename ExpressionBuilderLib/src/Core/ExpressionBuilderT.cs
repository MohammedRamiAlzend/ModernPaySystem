using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ExpressionBuilderLib.src.Core.Enums;
using ExpressionBuilderLib.src.Core.Interfaces;

namespace ExpressionBuilderLib.src.Core;

    /// <summary>
    /// A fluent expression builder for creating complex LINQ expressions
    /// </summary>
    /// <typeparam name="T">The type of entity to build expressions for</typeparam>
    public class ExpressionBuilder<T> : IExpressionBuilder<T>
    {
        private Expression<Func<T, bool>> _expression;
        private readonly ParameterExpression _parameter;

        /// <summary>
        /// Initializes a new instance of the ExpressionBuilder class
        /// </summary>
        public ExpressionBuilder()
        {
            _parameter = Expression.Parameter(typeof(T), "x");
            _expression = Expression.Lambda<Func<T, bool>>(Expression.Constant(true), _parameter);
        }

        /// <summary>
        /// Initializes a new instance with a starting expression
        /// </summary>
        /// <param name="initialExpression">The initial expression</param>
        public ExpressionBuilder(Expression<Func<T, bool>> initialExpression)
        {
            _parameter = initialExpression.Parameters[0];
            _expression = initialExpression;
        }

        /// <summary>
        /// Adds an AND condition to the expression
        /// </summary>
        /// <param name="predicate">The predicate to add</param>
        /// <returns>The current ExpressionBuilder instance</returns>
        public ExpressionBuilder<T> And(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) return this;

            var combined = ExpressionCombiner.Combine(
                _expression,
                predicate,
                LogicalOperator.And);

            _expression = combined;
            return this;
        }

        /// <summary>
        /// Adds an OR condition to the expression
        /// </summary>
        /// <param name="predicate">The predicate to add</param>
        /// <returns>The current ExpressionBuilder instance</returns>
        public ExpressionBuilder<T> Or(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) return this;

            var combined = ExpressionCombiner.Combine(
                _expression,
                predicate,
                LogicalOperator.Or);

            _expression = combined;
            return this;
        }

        /// <summary>
        /// Adds an AND NOT condition to the expression
        /// </summary>
        /// <param name="predicate">The predicate to negate and add</param>
        /// <returns>The current ExpressionBuilder instance</returns>
        public ExpressionBuilder<T> AndNot(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) return this;

            var combined = ExpressionCombiner.Combine(
                _expression,
                predicate,
                LogicalOperator.AndNot);

            _expression = combined;
            return this;
        }

        /// <summary>
        /// Adds an OR NOT condition to the expression
        /// </summary>
        /// <param name="predicate">The predicate to negate and add</param>
        /// <returns>The current ExpressionBuilder instance</returns>
        public ExpressionBuilder<T> OrNot(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null) return this;

            var combined = ExpressionCombiner.Combine(
                _expression,
                predicate,
                LogicalOperator.OrNot);

            _expression = combined;
            return this;
        }

        /// <summary>
        /// Adds a condition with the specified logical operator
        /// </summary>
        /// <param name="predicate">The predicate to add</param>
        /// <param name="logicalOperator">The logical operator to use</param>
        /// <returns>The current ExpressionBuilder instance</returns>
        public ExpressionBuilder<T> AddCondition(
            Expression<Func<T, bool>> predicate,
            LogicalOperator logicalOperator = LogicalOperator.And)
        {
            return logicalOperator switch
            {
                LogicalOperator.And => And(predicate),
                LogicalOperator.Or => Or(predicate),
                LogicalOperator.AndNot => AndNot(predicate),
                LogicalOperator.OrNot => OrNot(predicate),
                _ => And(predicate)
            };
        }

        /// <summary>
        /// Builds the final expression
        /// </summary>
        /// <returns>The combined expression</returns>
        public Expression<Func<T, bool>> Build()
        {
            return _expression;
        }

        /// <summary>
        /// Compiles the expression to a delegate
        /// </summary>
        /// <returns>The compiled delegate</returns>
        public Func<T, bool> Compile()
        {
            return _expression.Compile();
        }

        /// <summary>
        /// Resets the builder to its initial state
        /// </summary>
        public void Reset()
        {
            _expression = Expression.Lambda<Func<T, bool>>(
                Expression.Constant(true),
                _parameter);
        }

        /// <summary>
        /// Creates a clone of the current builder
        /// </summary>
        /// <returns>A new ExpressionBuilder with the same state</returns>
        public ExpressionBuilder<T> Clone()
        {
            return new ExpressionBuilder<T>(_expression);
        }

        /// <summary>
        /// Returns a string representation of the expression
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            return _expression?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Implicit conversion to Expression{Func{T, bool}}
        /// </summary>
        public static implicit operator Expression<Func<T, bool>>(ExpressionBuilder<T> builder)
        {
            return builder?.Build();
        }

        /// <summary>
        /// Implicit conversion to Func{T, bool}
        /// </summary>
        public static implicit operator Func<T, bool>(ExpressionBuilder<T> builder)
        {
            return builder?.Compile();
        }
}
