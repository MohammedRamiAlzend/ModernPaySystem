//using ExpressionBuilderLib.src.Core;
//using ExpressionBuilderLib.src.Core.Enums;
//using System.Linq.Expressions;

//namespace ExpressionBuilderLib;

//public class Class1
//{
//    public static void BasicUsageExample()
//    {
//        var personBuilder = new ExpressionBuilder<Person>();

//        personBuilder
//            .And(p => p.Age >= 18)
//            .And(p => p.Name.StartsWith("J"));

//        var expression = personBuilder.Build();

//        var compiledFunction = expression.Compile();

//        var people = new List<Person>
//        {
//            new Person { Name = "John", Age = 25 },
//            new Person { Name = "Jane", Age = 30 },
//            new Person { Name = "Bob", Age = 35 }
//        };

//        var filteredPeople = people.Where(compiledFunction).ToList();

//        Console.WriteLine("Filtered people:");
//        foreach (var person in filteredPeople)
//        {
//            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
//        }
//    }

//    public static void AdvancedUsageExample()
//    {
//        var filters = new Dictionary<string, object>
//        {
//            { "Age", 25 },
//            { "Name", "John" }
//        };

//        var expression = DynamicExpressionBuilder.BuildFromFilters<Person>(filters);

//        var compiledFunction = expression.Compile();

//        var people = new List<Person>
//        {
//            new Person { Name = "John", Age = 25 },
//            new Person { Name = "Jane", Age = 30 },
//            new Person { Name = "John", Age = 20 }
//        };

//        var filteredPeople = people.Where(compiledFunction).ToList();

//        Console.WriteLine("\nAdvanced filtering results:");
//        foreach (var person in filteredPeople)
//        {
//            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
//        }
//    }

//    public static void ExpressionCombiningExample()
//    {
//        Expression<Func<Person, bool>> ageExpression = p => p.Age > 20;
//        Expression<Func<Person, bool>> nameExpression = p => p.Name.Contains("o");

//        var combinedExpression = ExpressionCombiner.Combine(ageExpression, nameExpression, LogicalOperator.And);

//        var builder = new ExpressionBuilder<Person>();
//        builder.AddCondition(ageExpression, LogicalOperator.And)
//               .AddCondition(nameExpression, LogicalOperator.And);

//        var finalExpression = builder.Build();
//        var compiledFunction = finalExpression.Compile();

//        var people = new List<Person>
//        {
//            new Person { Name = "John", Age = 25 },
//            new Person { Name = "Jane", Age = 30 },
//            new Person { Name = "Bob", Age = 18 },
//            new Person { Name = "Tom", Age = 22 }
//        };

//        var filteredPeople = people.Where(compiledFunction).ToList();

//        Console.WriteLine("\nCombined expression results:");
//        foreach (var person in filteredPeople)
//        {
//            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
//        }
//    }

//    public static void StringComparisonExample()
//    {
//        var containsExpression = DynamicExpressionBuilder.CreateStringContainsExpression<Person>("Name", "oh");

//        var compiledFunction = containsExpression.Compile();

//        var people = new List<Person>
//        {
//            new Person { Name = "John", Age = 25 },
//            new Person { Name = "Jane", Age = 30 },
//            new Person { Name = "Bob", Age = 18 },
//            new Person { Name = "Johnny", Age = 22 }
//        };

//        var filteredPeople = people.Where(compiledFunction).ToList();

//        Console.WriteLine("\nString comparison results:");
//        foreach (var person in filteredPeople)
//        {
//            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
//        }
//    }

//    public static void RequestFilteringExample()
//    {
//        Guid currentUserId = Guid.NewGuid();

//        var requestBuilder = new ExpressionBuilder<Request>();

//        requestBuilder.And(r => r.ApproverId == currentUserId);

//        var expression = requestBuilder.Build();

//        var compiledFunction = expression.Compile();

//        var allRequests = new List<Request>
//        {
//            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid() },
//            new Request { Id = Guid.NewGuid(), ApproverId = Guid.NewGuid(), RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid() },
//            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid() }
//        };

//        var receivedRequests = allRequests.Where(compiledFunction).ToList();

//        Console.WriteLine($"\nReceived requests for user {currentUserId}:");
//        foreach (var request in receivedRequests)
//        {
//            Console.WriteLine($"Request ID: {request.Id}, Approver ID: {request.ApproverId}");
//        }
//    }

//    public static void RequestPaginationExample()
//    {
//        Guid currentUserId = Guid.NewGuid();

//        var requestBuilder = new ExpressionBuilder<Request>();

//        requestBuilder.And(r => r.ApproverId == currentUserId);

//        var expression = requestBuilder.Build();

//        var allRequests = new List<Request>
//        {
//            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-1) },
//            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-2) },
//            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-3) },
//            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-4) },
//            new Request { Id = Guid.NewGuid(), ApproverId = currentUserId, RequesterId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), CreatedAt = DateTime.Now.AddDays(-5) }
//        };

//        var sortedRequests = allRequests.OrderByDescending(r => r.CreatedAt).ToList();

//        int pageNumber = 1;
//        int pageSize = 2;
//        var pagedRequests = sortedRequests
//            .Skip((pageNumber - 1) * pageSize)
//            .Take(pageSize)
//            .ToList();

//        Console.WriteLine($"\nPaginated requests for user {currentUserId} (Page {pageNumber}, Size {pageSize}):");
//        foreach (var request in pagedRequests)
//        {
//            Console.WriteLine($"Request ID: {request.Id}, Approver ID: {request.ApproverId}, Created At: {request.CreatedAt}");
//        }

//        var totalCount = sortedRequests.Count;
//        Console.WriteLine($"Total requests: {totalCount}");
//        Console.WriteLine($"Total pages: {(int)Math.Ceiling((double)totalCount / pageSize)}");
//    }
//}

//public class Request
//{
//    public Guid Id { get; set; }
//    public Guid TemplateId { get; set; }
//    public Guid RequesterId { get; set; }
//    public Guid ApproverId { get; set; }
//    public string ContentAsJson { get; set; } = string.Empty;
//    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    public DateTime? UpdatedAt { get; set; }
//    public RequestStatus Status { get; set; } = RequestStatus.Pending;
//}

//public enum RequestStatus
//{
//    Pending,
//    Approved,
//    Rejected,
//    Cancelled
//}

//public class Person
//{
//    public string Name { get; set; }
//    public int Age { get; set; }
//    public string Email { get; set; } = string.Empty;
//    public DateTime DateOfBirth { get; set; }
//}
