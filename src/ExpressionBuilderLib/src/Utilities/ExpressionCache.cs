namespace ExpressionBuilderLib.src.Utilities;

using System.Collections.Concurrent;
using System.Linq.Expressions;

/// <summary>
/// Provides caching for compiled expressions
/// </summary>
public static class ExpressionCache
{
    private static readonly ConcurrentDictionary<string, Expression> _cache = new();

    public static Expression<Func<T, bool>> GetOrCreate<T>(
        string cacheKey,
        Func<Expression<Func<T, bool>>> expressionFactory)
    {
        if (_cache.TryGetValue(cacheKey, out var cachedExpression))
        {
            return (Expression<Func<T, bool>>)cachedExpression;
        }

        var expression = expressionFactory();
        _cache.TryAdd(cacheKey, expression);

        return expression;
    }

    public static void Clear()
    {
        _cache.Clear();
    }

    public static void Remove(string cacheKey)
    {
        _cache.TryRemove(cacheKey, out _);
    }

    public static int Count => _cache.Count;
}
