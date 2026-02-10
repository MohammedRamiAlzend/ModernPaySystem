using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core;
using ExpressionBuilderLib.src.Core.Enums;

namespace ExpressionBuilderLib;

/// <summary>
/// Sample class demonstrating how to use the ExpressionBuilderLib functionality
/// </summary>
public class Class1
{
    /// <summary>
    /// Demonstrates basic usage of the ExpressionBuilder
    /// </summary>
    public static void BasicUsageExample()
    {
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

        // Example usage with sample data
        var people = new List<Person>
        {
            new Person { Name = "John", Age = 25 },
            new Person { Name = "Jane", Age = 30 },
            new Person { Name = "Bob", Age = 35 }
        };

        // Filter the list using the expression
        var filteredPeople = people.Where(compiledFunction).ToList();

        Console.WriteLine("Filtered people:");
        foreach (var person in filteredPeople)
        {
            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
        }
    }

    /// <summary>
    /// Demonstrates advanced usage with DynamicExpressionBuilder.
    /// </summary>
    public static void AdvancedUsageExample()
    {
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

        // Example usage with sample data
        var people = new List<Person>
        {
            new Person { Name = "John", Age = 25 },
            new Person { Name = "Jane", Age = 30 },
            new Person { Name = "John", Age = 20 }
        };

        var filteredPeople = people.Where(compiledFunction).ToList();

        Console.WriteLine("\nAdvanced filtering results:");
        foreach (var person in filteredPeople)
        {
            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
        }
    }

    /// <summary>
    /// Demonstrates combining expressions with different logical operators
    /// </summary>
    public static void ExpressionCombiningExample()
    {
        // Create individual expressions
        Expression<Func<Person, bool>> ageExpression = p => p.Age > 20;
        Expression<Func<Person, bool>> nameExpression = p => p.Name.Contains("o");

        // Combine expressions using ExpressionCombiner
        var combinedExpression = ExpressionCombiner.Combine(ageExpression, nameExpression, LogicalOperator.And);
        
        // Or use the ExpressionBuilder to combine expressions
        var builder = new ExpressionBuilder<Person>();
        builder.AddCondition(ageExpression, LogicalOperator.And)
               .AddCondition(nameExpression, LogicalOperator.And);

        var finalExpression = builder.Build();
        var compiledFunction = finalExpression.Compile();

        // Example usage with sample data
        var people = new List<Person>
        {
            new Person { Name = "John", Age = 25 },
            new Person { Name = "Jane", Age = 30 },
            new Person { Name = "Bob", Age = 18 },
            new Person { Name = "Tom", Age = 22 }
        };

        var filteredPeople = people.Where(compiledFunction).ToList();

        Console.WriteLine("\nCombined expression results:");
        foreach (var person in filteredPeople)
        {
            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
        }
    }

    /// <summary>
    /// Demonstrates string comparison operations
    /// </summary>
    public static void StringComparisonExample()
    {
        // Create an expression for string contains operation
        var containsExpression = DynamicExpressionBuilder.CreateStringContainsExpression<Person>("Name", "oh");
        
        var compiledFunction = containsExpression.Compile();

        // Example usage with sample data
        var people = new List<Person>
        {
            new Person { Name = "John", Age = 25 },
            new Person { Name = "Jane", Age = 30 },
            new Person { Name = "Bob", Age = 18 },
            new Person { Name = "Johnny", Age = 22 }
        };

        var filteredPeople = people.Where(compiledFunction).ToList();

        Console.WriteLine("\nString comparison results:");
        foreach (var person in filteredPeople)
        {
            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
        }
    }
}

/// <summary>
/// Sample class to demonstrate the expression builder functionality
/// </summary>
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}
