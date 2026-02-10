# ExpressionBuilderLib - User Guide

## Overview

ExpressionBuilderLib is a powerful library for dynamically building LINQ expressions at runtime. It enables developers to create complex queries programmatically, which is especially useful for building dynamic filters, search functionality, and conditional logic in applications.

## Key Features

- **Fluent API**: Easy-to-use fluent interface for building expressions
- **Dynamic Filtering**: Build expressions from dictionaries or runtime data
- **Multiple Operators**: Support for various comparison and logical operators
- **Type Safety**: Strongly typed expressions with compile-time checking
- **Performance**: Efficient expression compilation and caching

## Installation

To use ExpressionBuilderLib in your project, simply add a reference to the library:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="path/to/ExpressionBuilderLib.csproj" />
  </ItemGroup>
</Project>
```

## Basic Usage

### 1. Simple Expression Building

```csharp
using ExpressionBuilderLib.src.Core;

// Create an expression builder for a Person class
var personBuilder = new ExpressionBuilder<Person>();

// Add conditions to the expression
personBuilder
    .And(p => p.Age >= 18)
    .And(p => p.Name.StartsWith("J"));

// Build the expression
var expression = personBuilder.Build();

// Compile the expression to a function
var compiledFunction = expression.Compile();

// Use the function to filter data
var people = GetPeople(); // Your data source
var filteredPeople = people.Where(compiledFunction).ToList();
```

### 2. Using Dynamic Expression Builder

```csharp
using ExpressionBuilderLib.src.Core;
using System.Collections.Generic;

// Create filters dynamically
var filters = new Dictionary<string, object>
{
    { "Age", 25 },
    { "Name", "John" }
};

// Build an expression from the filters
var expression = DynamicExpressionBuilder.BuildFromFilters<Person>(filters);

// Compile and use the expression
var compiledFunction = expression.Compile();
var filteredPeople = people.Where(compiledFunction).ToList();
```

### 3. Combining Multiple Expressions

```csharp
using ExpressionBuilderLib.src.Core;
using ExpressionBuilderLib.src.Core.Enums;
using System.Linq.Expressions;

// Create individual expressions
Expression<Func<Person, bool>> ageExpression = p => p.Age > 20;
Expression<Func<Person, bool>> nameExpression = p => p.Name.Contains("o");

// Combine expressions using ExpressionCombiner
var combinedExpression = ExpressionCombiner.Combine(
    ageExpression, 
    nameExpression, 
    LogicalOperator.And);

// Or use the ExpressionBuilder to combine expressions
var builder = new ExpressionBuilder<Person>();
builder.AddCondition(ageExpression, LogicalOperator.And)
       .AddCondition(nameExpression, LogicalOperator.And);

var finalExpression = builder.Build();
```

## Advanced Usage

### 1. String Comparison Operations

```csharp
// Create an expression for string contains operation
var containsExpression = DynamicExpressionBuilder.CreateStringContainsExpression<Person>(
    "Name", "oh");

// Create an expression for string starts with operation
var startsWithExpression = DynamicExpressionBuilder.CreateStringContainsExpression<Person>(
    "Name", "Jo");

// Create an expression for string ends with operation
var endsWithExpression = DynamicExpressionBuilder.CreateStringEndsWithExpression<Person>(
    parameter, property, constant, StringComparisonMode.OrdinalIgnoreCase);
```

### 2. Complex Property Paths

```csharp
// Handle nested properties (e.g., "Address.City")
var expression = DynamicExpressionBuilder.CreatePropertyExpression<Person>(
    "Address.City", 
    "New York", 
    ComparisonOperator.Equal);
```

### 3. Different Comparison Operators

```csharp
using ExpressionBuilderLib.src.Core.Enums;

// Various comparison options
var equalExpression = DynamicExpressionBuilder.CreatePropertyExpression<Person>(
    "Age", 25, ComparisonOperator.Equal);

var greaterThanExpression = DynamicExpressionBuilder.CreatePropertyExpression<Person>(
    "Age", 18, ComparisonOperator.GreaterThan);

var containsExpression = DynamicExpressionBuilder.CreatePropertyExpression<Person>(
    "Name", "John", ComparisonOperator.Contains);
```

## Core Components

### ExpressionBuilder<T>

The main class for building expressions fluently. Provides methods like `And()`, `Or()`, `AndNot()`, and `OrNot()` to combine conditions.

### DynamicExpressionBuilder

Used for creating expressions from runtime data like property names and values. Particularly useful for building filters from user input or configuration.

### ExpressionCombiner

Utility class for combining multiple expressions using logical operators.

### Enums

- `ComparisonOperator`: Equal, NotEqual, GreaterThan, Contains, StartsWith, etc.
- `LogicalOperator`: And, Or, AndNot, OrNot
- `StringComparisonMode`: Ordinal, OrdinalIgnoreCase, CurrentCulture, etc.

## Practical Examples

### Example 1: Search Functionality

```csharp
public List<Product> SearchProducts(string searchTerm, decimal? minPrice, decimal? maxPrice)
{
    var builder = new ExpressionBuilder<Product>();
    
    if (!string.IsNullOrEmpty(searchTerm))
    {
        var nameContains = DynamicExpressionBuilder.CreateStringContainsExpression<Product>("Name", searchTerm);
        var descContains = DynamicExpressionBuilder.CreateStringContainsExpression<Product>("Description", searchTerm);
        builder.Or(nameContains).Or(descContains);
    }
    
    if (minPrice.HasValue)
    {
        builder.And(p => p.Price >= minPrice.Value);
    }
    
    if (maxPrice.HasValue)
    {
        builder.And(p => p.Price <= maxPrice.Value);
    }
    
    var compiledFilter = builder.Build().Compile();
    return products.Where(compiledFilter).ToList();
}
```

### Example 2: Dynamic Filtering from API Parameters

```csharp
public IQueryable<T> ApplyFilters<T>(IQueryable<T> query, Dictionary<string, object> filters)
{
    if (filters == null || !filters.Any())
        return query;
        
    var expression = DynamicExpressionBuilder.BuildFromFilters<T>(filters);
    return query.Where(expression);
}
```

## Best Practices

1. **Cache Compiled Expressions**: If you're using the same expression multiple times, cache the compiled function to improve performance.

2. **Validate Input**: When building expressions from user input, validate the property names and values to prevent injection attacks.

3. **Handle Null Values**: The library handles null values appropriately, but be aware of how they affect your expressions.

4. **Use Strong Typing**: Leverage the strongly-typed nature of the library to catch errors at compile time.

## Performance Tips

- Reuse expression builders when possible
- Cache compiled expressions for repeated use
- Consider using ExpressionCache for frequently used expressions
- Minimize the creation of complex nested expressions when possible

## Troubleshooting

### Common Issues:

1. **Invalid Property Names**: Ensure property names match exactly (case-sensitive) with the target type.
2. **Type Mismatches**: Make sure the value types match the property types, or they can be converted.
3. **Complex Nested Properties**: The library supports "Parent.Child.Property" notation for navigating object hierarchies.

## Extending the Library

The library is designed to be extensible. You can create custom extension methods for ExpressionBuilder or implement your own expression combinators using the provided base classes.

## Conclusion

ExpressionBuilderLib provides a flexible and powerful way to build LINQ expressions dynamically. Its fluent API and extensive feature set make it ideal for creating dynamic queries, filtering systems, and other scenarios where expressions need to be constructed at runtime.