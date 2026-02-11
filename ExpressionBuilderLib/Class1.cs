using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core;
using ExpressionBuilderLib.src.Core.Enums;

namespace ExpressionBuilderLib;

/// <summary>
/// Sample class demonstrating how to use the ExpressionBuilderLib functionality.
/// </summary>
public class Class1
{
    /// <summary>
    /// Demonstrates basic usage of the ExpressionBuilder.
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
    
    /// <summary>
    /// Demonstrates how to use ExpressionBuilderLib to filter requests for a specific user
    /// This example shows how to implement a method similar to what would be used in RequestService
    /// </summary>
    public static void RequestFilteringExample()
    {
        // Simulate getting the current user ID (would come from IHttpContextServiceManager in real implementation)
        Guid currentUserId = Guid.NewGuid(); // In real implementation: httpContextServiceManager.GetCurrentUserId()

        // Create an expression builder for Request entities
        var requestBuilder = new ExpressionBuilder<Request>();

        // Add a condition to filter requests where the ApproverId matches the current user ID
        // This represents requests that were sent TO the current user (they are the approver)
        requestBuilder.And(r => r.ApproverId == currentUserId);

        // Build the expression
        var expression = requestBuilder.Build();

        // Compile the expression to a function
        var compiledFunction = expression.Compile();

        // Example usage with sample data (in real implementation, this would come from the repository)
        var allRequests = new List<Request>
        {
            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid() },
            new Request { Id = Guid.NewGuid(), ApproverId = Guid.NewGuid(), RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid() }, // Different approver
            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid() }
        };

        // Filter the requests using the expression
        var receivedRequests = allRequests.Where(compiledFunction).ToList();

        Console.WriteLine($"\nReceived requests for user {currentUserId}:");
        foreach (var request in receivedRequests)
        {
            Console.WriteLine($"Request ID: {request.Id}, Approver ID: {request.ApproverId}");
        }
    }

    /// <summary>
    /// Demonstrates how to use ExpressionBuilderLib with pagination for requests
    /// This example shows how to implement a method similar to what would be used in RequestService.
    /// </summary>
    public static void RequestPaginationExample()
    {
        // Simulate getting the current user ID (would come from IHttpContextServiceManager in real implementation)
        Guid currentUserId = Guid.NewGuid(); // In real implementation: httpContextServiceManager.GetCurrentUserId()

        // Create an expression builder for Request entities
        var requestBuilder = new ExpressionBuilder<Request>();

        // Add a condition to filter requests where the ApproverId matches the current user ID
        // This represents requests that were sent TO the current user (they are the approver)
        requestBuilder.And(r => r.ApproverId == currentUserId);

        // Build the expression
        var expression = requestBuilder.Build();

        // Example usage with sample data (in real implementation, this would come from the repository)
        var allRequests = new List<Request>
        {
            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-1) },
            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-2) },
            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-3) },
            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-4) },
            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-5) }
        };

        // Sort the requests by creation date (newest first)
        var sortedRequests = allRequests.OrderByDescending(r => r.CreatedAt).ToList();

        // Apply pagination
        int pageNumber = 1;
        int pageSize = 2;
        var pagedRequests = sortedRequests
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        Console.WriteLine($"\nPaginated requests for user {currentUserId} (Page {pageNumber}, Size {pageSize}):");
        foreach (var request in pagedRequests)
        {
            Console.WriteLine($"Request ID: {request.Id}, Approver ID: {request.ApproverId}, Created At: {request.CreatedAt}");
        }

        // Show total count
        var totalCount = sortedRequests.Count;
        Console.WriteLine($"Total requests: {totalCount}");
        Console.WriteLine($"Total pages: {(int)Math.Ceiling((double)totalCount / pageSize)}");
    }
}

/// <summary>
/// Sample Request class to demonstrate the expression builder functionality
/// In the actual implementation, this would be from ModernPaySystem.Domain.Entities.TransactionSystemEntities
/// </summary>
public class Request
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid ApproverId { get; set; }
    public string ContentAsJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
}

/// <summary>
/// Sample RequestStatus enum to demonstrate the expression builder functionality
/// </summary>
public enum RequestStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
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
