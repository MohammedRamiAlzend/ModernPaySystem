# result-helper Skill Definition

## Skill Name: `result-helper`

## Description
A specialized skill for working with the Result pattern in ModernPaySystem. Provides structured workflows for implementing, reviewing, and troubleshooting Result<T> pattern usage across services, controllers, and tests.

## Invocation
```
/result-helper
/result-helper create service method
/result-helper create controller action
/result-helper review code
/result-helper explain errors
```

## Skill Capabilities

### 1. Create Service Method Workflow
When invoked with "create service method" or similar:
1. Ask user for:
   - Entity/DTO type (return type)
   - Method name
   - Operation type (Create, Read, Update, Delete)
   - Validation requirements
   - Business rules to check

2. Generate complete service method with:
   - Proper signature: `public async Task<Result<TDto>> MethodNameAsync(...)`
   - Input validation using **ApplicationErrors** (NEVER create new Error instances)
   - Entity existence checks using **ApplicationErrors**
   - Conflict detection using **ApplicationErrors**
   - Appropriate success marker (`Result.Success`, `Result.Created`, `Result.Updated`, `Result.Deleted`)
   - Comments explaining each step

3. ⚠️ **IMPORTANT**: Only use errors from `ApplicationErrors.cs`. If a needed error doesn't exist, suggest adding it to ApplicationErrors.cs first.

4. Reference: `.qwen/result-pattern-guide.md` → "Workflow 1: Creating a Service Method"

### 2. Create Controller Action Workflow
When invoked with "create controller action" or similar:
1. Ask user for:
   - HTTP method (GET, POST, PUT, DELETE)
   - Action name
   - Route parameters
   - Request body type (if applicable)
   - Service method to call

2. Generate complete controller action with:
   - Proper attributes (`[HttpGet]`, `[HttpPost]`, etc.)
   - Route constraints (`{id:guid}`)
   - Service call
   - Result conversion (`result.ToActionResult()`)
   - Optional location headers for Created responses

3. Reference: `.qwen/result-pattern-guide.md` → "Workflow 2: Creating a Controller Action"

### 3. Review Code Workflow
When invoked with "review" or "check code":
1. Ask user for file path or code snippet
2. Check for:
   - ✅ Proper use of `Result<T>` return types
   - ✅ Error checking (`if (result.IsError)`)
   - ✅ Correct `ToActionResult()` usage
   - ✅ Using **ApplicationErrors** for all error returns
   - ❌ Creating new Error instances outside ApplicationErrors.cs (ANTI-PATTERN)
   - ✅ Appropriate ErrorKind selection from ApplicationErrors
   - ✅ No exception throwing for expected failures
   - ✅ Early returns for validation
   - ❌ Anti-patterns (see guide)

3. Provide specific feedback with:
   - What's correct
   - What needs improvement
   - Suggested fixes with code examples

4. Reference: `.qwen/result-pattern-guide.md` → "Common Patterns & Anti-Patterns"

### 4. Explain Error Types Workflow
When invoked with "explain errors" or "which error to use":
1. Show error type decision tree:
   - Invalid input? → `Error.Validation` (400)
   - Entity missing? → `Error.NotFound` (404)
   - Duplicate data? → `Error.Conflict` (409)
   - Auth issue? → `Error.Unauthorized` (401)
   - Permission issue? → `Error.Forbidden` (403)
   - Unknown issue? → `Error.Unexpected` (500)

2. Provide examples for each error type
3. Reference: `.qwen/result-pattern-guide.md` → "Error Type Guidelines"

### 5. Testing Workflow
When invoked with "create tests" or "how to test":
1. Generate unit test examples for:
   - Success scenarios
   - Error scenarios (validation, not found, conflict, etc.)
   - Assertions for `IsSuccess`, `IsError`, `TopError`, `Value`

2. Reference: `.qwen/result-pattern-guide.md` → "Testing Result Pattern"

## Skill Behavior

### When User Invokes Skill
1. Greet user and present options:
   ```
   I'm the Result Pattern Helper! What would you like to do?
   
   1. Create a service method with Result pattern
   2. Create a controller action with Result pattern
   3. Review existing Result pattern usage
   4. Explain error types and when to use them
   5. Generate unit tests for Result pattern
   6. Show quick reference
   ```

2. Wait for user selection

3. Execute appropriate workflow based on selection

4. Ask follow-up questions as needed to gather context

5. Generate code/examples based on user input

6. Offer to implement the code in actual files if user confirms

## Reference Material
- Full pattern guide: `.qwen/result-pattern-guide.md`
- Result<T>: `ModernPaySystem.Domain/Commons/ResultOfT.cs`
- Error: `ModernPaySystem.Domain/Commons/Error.cs`
- **ApplicationErrors**: `ModernPaySystem.Domain/Commons/ApplicationErrors.cs` ⚠️ **All errors MUST be defined here**
- Extensions: `ModernPaySystem.Infrastructure/Extensions/ResultExtensions.cs`
- Examples: `ModernPaySystem/Controllers/*.cs`

## Example Interactions

### Example 1: Create Service Method
```
User: /result-helper create service method

Skill: I'll help you create a service method with the Result pattern. 
       What entity/DTO will this method work with?

User: UserDto

Skill: What's the method name?

User: CreateUserAsync

Skill: What type of operation is this? (Create/Read/Update/Delete)

User: Create

Skill: [Generates complete service method with validation, error handling, and Result.Created return]
```

### Example 2: Review Code
```
User: /result-helper review ModernPaySystem/Controllers/UsersController.cs

Skill: [Reads file and provides review:]
       ✅ Good: Proper use of ToActionResult()
       ⚠️ Issue: Line 45 - accessing result.Value without checking IsError
       💡 Fix: Add error check before accessing Value
       [Provides corrected code]
```

### Example 3: Error Type Guidance
```
User: /result-helper which error should I use when email already exists?

Skill: For duplicate scenarios, use Error.Conflict (HTTP 409):
       
       return Error.Conflict(
           code: "EmailAlreadyExists",
           description: $"A user with email {email} already exists."
       );
       
       This returns 409 Conflict, which is the standard HTTP status for duplicate resource attempts.
```

## Notes
- Always reference actual project files when generating code
- Follow naming conventions from AGENTS.md
- Use project's existing patterns as examples
- Offer to write code to files when user confirms the approach
- Provide both simple and advanced examples as appropriate
