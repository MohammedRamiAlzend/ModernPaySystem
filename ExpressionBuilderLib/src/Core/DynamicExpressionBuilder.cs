using System.Linq.Expressions;
using System.Reflection;
using ExpressionBuilderLib.src.Core.Enums;

namespace ExpressionBuilderLib.src.Core;

/// <summary>
/// Builds expressions dynamically from property names and values.
/// </summary>
public static class DynamicExpressionBuilder
{
    /// <summary>
    /// Creates an expression for a property comparison.
    /// </summary>
    public static Expression<Func<T, bool>> CreatePropertyExpression<T>(
        string propertyName,
        object value,
        ComparisonOperator comparison = ComparisonOperator.Equal,
        StringComparisonMode stringComparison = StringComparisonMode.OrdinalIgnoreCase)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        // Handle nested properties (e.g., "Address.City")
        Expression property = parameter;
        foreach (var part in propertyName.Split('.'))
        {
            property = Expression.PropertyOrField(property, part);
        }

        // Handle null values
        if (value == null)
        {
            return CreateNullComparisonExpression<T>(parameter, property, comparison);
        }

        // Convert value to the property type if needed
        var propertyType = property.Type;
        var convertedValue = ConvertValue(value, propertyType);
        var constant = Expression.Constant(convertedValue, propertyType);

        return CreateComparisonExpression<T>(
            parameter, property, constant, comparison, stringComparison);
    }

    /// <summary>
    /// Creates expressions from a dictionary of filters
    /// </summary>
    public static Expression<Func<T, bool>> BuildFromFilters<T>(
        IDictionary<string, object> filters,
        LogicalOperator logicalOperator = LogicalOperator.And,
        StringComparisonMode stringComparison = StringComparisonMode.OrdinalIgnoreCase)
    {
        var builder = new ExpressionBuilder<T>();

        foreach (var filter in filters)
        {
            var expression = CreatePropertyExpression<T>(
                filter.Key,
                filter.Value,
                ComparisonOperator.Equal,
                stringComparison);

            builder.AddCondition(expression, logicalOperator);
        }

        return builder.Build();
    }

    /// <summary>
    /// Creates an expression for string contains operation
    /// </summary>
    public static Expression<Func<T, bool>> CreateStringContainsExpression<T>(
        string propertyName,
        string value,
        StringComparisonMode stringComparison = StringComparisonMode.OrdinalIgnoreCase)
    {
        if (string.IsNullOrEmpty(value))
            return Expression.Lambda<Func<T, bool>>(Expression.Constant(true), Expression.Parameter(typeof(T)));

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.PropertyOrField(parameter, propertyName);

        // Null check
        var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

        // Get the appropriate Contains method based on string comparison mode
        MethodInfo containsMethod = GetStringContainsMethod(stringComparison);

        var containsCall = Expression.Call(
            property,
            containsMethod,
            Expression.Constant(value),
            Expression.Constant(GetStringComparison(stringComparison)));

        var andExpr = Expression.AndAlso(nullCheck, containsCall);

        return Expression.Lambda<Func<T, bool>>(andExpr, parameter);
    }

    private static Expression<Func<T, bool>> CreateNullComparisonExpression<T>(
        ParameterExpression parameter,
        Expression property,
        ComparisonOperator comparison)
    {
        return comparison switch
        {
            ComparisonOperator.IsNull =>
                Expression.Lambda<Func<T, bool>>(
                    Expression.Equal(property, Expression.Constant(null, property.Type)),
                    parameter),
            ComparisonOperator.IsNotNull =>
                Expression.Lambda<Func<T, bool>>(
                    Expression.NotEqual(property, Expression.Constant(null, property.Type)),
                    parameter),
            _ => throw new ArgumentException(
                $"Null value can only be used with {nameof(ComparisonOperator.IsNull)} or {nameof(ComparisonOperator.IsNotNull)}")
        };
    }

    private static Expression<Func<T, bool>> CreateComparisonExpression<T>(
        ParameterExpression parameter,
        Expression property,
        ConstantExpression constant,
        ComparisonOperator comparison,
        StringComparisonMode stringComparison)
    {
        return comparison switch
        {
            ComparisonOperator.Equal =>
                CreateEqualExpression<T>(parameter, property, constant, stringComparison),
            ComparisonOperator.NotEqual =>
                CreateNotEqualExpression<T>(parameter, property, constant, stringComparison),
            ComparisonOperator.GreaterThan =>
                Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(property, constant), parameter),
            ComparisonOperator.GreaterThanOrEqual =>
                Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(property, constant), parameter),
            ComparisonOperator.LessThan =>
                Expression.Lambda<Func<T, bool>>(Expression.LessThan(property, constant), parameter),
            ComparisonOperator.LessThanOrEqual =>
                Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(property, constant), parameter),
            ComparisonOperator.Contains =>
                CreateStringContainsExpression<T>(GetPropertyName(property), constant.Value.ToString(), stringComparison),
            ComparisonOperator.StartsWith =>
                CreateStringStartsWithExpression<T>(parameter, property, constant, stringComparison),
            ComparisonOperator.EndsWith =>
                CreateStringEndsWithExpression<T>(parameter, property, constant, stringComparison),
            _ => throw new NotSupportedException($"Comparison operator '{comparison}' is not supported")
        };
    }

    private static Expression<Func<T, bool>> CreateEqualExpression<T>(
        ParameterExpression parameter,
        Expression property,
        ConstantExpression constant,
        StringComparisonMode stringComparison)
    {
        if (property.Type == typeof(string))
        {
            // For strings, we need to handle case sensitivity
            var method = typeof(string).GetMethod(
                "Equals",
                new[] { typeof(string), typeof(StringComparison) });

            var equalsCall = Expression.Call(
                property,
                method,
                constant,
                Expression.Constant(GetStringComparison(stringComparison)));

            return Expression.Lambda<Func<T, bool>>(equalsCall, parameter);
        }

        return Expression.Lambda<Func<T, bool>>(Expression.Equal(property, constant), parameter);
    }

    private static Expression<Func<T, bool>> CreateNotEqualExpression<T>(
        ParameterExpression parameter,
        Expression property,
        ConstantExpression constant,
        StringComparisonMode stringComparison)
    {
        var equalExpression = CreateEqualExpression<T>(parameter, property, constant, stringComparison);
        var notExpression = Expression.Not(equalExpression.Body);
        return Expression.Lambda<Func<T, bool>>(notExpression, parameter);
    }

    private static Expression<Func<T, bool>> CreateStringStartsWithExpression<T>(
        ParameterExpression parameter,
        Expression property,
        ConstantExpression constant,
        StringComparisonMode stringComparison)
    {
        var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

        var startsWithMethod = typeof(string).GetMethod(
            "StartsWith",
            new[] { typeof(string), typeof(StringComparison) });

        var startsWithCall = Expression.Call(
            property,
            startsWithMethod,
            constant,
            Expression.Constant(GetStringComparison(stringComparison)));

        var andExpr = Expression.AndAlso(nullCheck, startsWithCall);

        return Expression.Lambda<Func<T, bool>>(andExpr, parameter);
    }

    private static Expression<Func<T, bool>> CreateStringEndsWithExpression<T>(
        ParameterExpression parameter,
        Expression property,
        ConstantExpression constant,
        StringComparisonMode stringComparison)
    {
        var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

        var endsWithMethod = typeof(string).GetMethod(
            "EndsWith",
            new[] { typeof(string), typeof(StringComparison) });

        var endsWithCall = Expression.Call(
            property,
            endsWithMethod,
            constant,
            Expression.Constant(GetStringComparison(stringComparison)));

        var andExpr = Expression.AndAlso(nullCheck, endsWithCall);

        return Expression.Lambda<Func<T, bool>>(andExpr, parameter);
    }

    private static MethodInfo GetStringContainsMethod(StringComparisonMode stringComparison)
    {
        return typeof(string).GetMethod(
            "Contains",
            new[] { typeof(string), typeof(StringComparison) });
    }

    private static StringComparison GetStringComparison(StringComparisonMode mode)
    {
        return mode switch
        {
            StringComparisonMode.Ordinal => StringComparison.Ordinal,
            StringComparisonMode.OrdinalIgnoreCase => StringComparison.OrdinalIgnoreCase,
            StringComparisonMode.CurrentCulture => StringComparison.CurrentCulture,
            StringComparisonMode.CurrentCultureIgnoreCase => StringComparison.CurrentCultureIgnoreCase,
            StringComparisonMode.InvariantCulture => StringComparison.InvariantCulture,
            StringComparisonMode.InvariantCultureIgnoreCase => StringComparison.InvariantCultureIgnoreCase,
            _ => StringComparison.OrdinalIgnoreCase
        };
    }

    private static object ConvertValue(object value, Type targetType)
    {
        if (value == null)
            return null;

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value.ToString());

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, value.ToString());
        }

        return Convert.ChangeType(value, targetType);
    }

    private static string GetPropertyName(Expression property)
    {
        var memberExpression = property as MemberExpression;
        return memberExpression?.Member.Name ?? string.Empty;
    }
}
