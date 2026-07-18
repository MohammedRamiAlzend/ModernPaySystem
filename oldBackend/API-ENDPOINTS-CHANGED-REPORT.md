# API Endpoints Changed Report

## Overview
This report documents all API endpoint changes resulting from the refactoring of `ResponseTransaction` to `RequestTransaction`. The changes align the API with the correct business workflow where transactions belong to **Requests** (not Responses).

**Date**: April 12, 2026  
**Base Route**: `/api/RequestTransactions` (previously `/api/ResponseTransactions`)

---

## Controller Changes

### Controller Name
- **Before**: `ResponseTransactionsController`
- **After**: `RequestTransactionsController`

### Route Base
- **Before**: `/api/ResponseTransactions`
- **After**: `/api/RequestTransactions`

### Permission Strings
- **Before**: `response-transactions.*`
- **After**: `request-transactions.*`

---

## Endpoints Comparison

### 1. Get Transaction By ID

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `GET` | `GET` |
| **Route** | `/api/ResponseTransactions/{id}` | `/api/RequestTransactions/{id}` |
| **Permission** | `response-transactions.get-by-id` | `request-transactions.get-by-id` |
| **Parameter** | `Guid id` | `Guid id` |
| **Response** | `ResponseTransactionDto` | `RequestTransactionDto` |

**Example Request**:
```bash
GET /api/RequestTransactions/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer <token>
```

---

### 2. Get Transactions By Request ID

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `GET` | `GET` |
| **Route** | `/api/ResponseTransactions/by-response/{responseId}` | `/api/RequestTransactions/by-request/{requestId}` |
| **Permission** | `response-transactions.get-by-response-id` | `request-transactions.get-by-request-id` |
| **Parameter** | `Guid responseId` | `Guid requestId` |
| **Response** | `List<ResponseTransactionDto>` | `List<RequestTransactionDto>` |

**Example Request**:
```bash
GET /api/RequestTransactions/by-request/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer <token>
```

---

### 3. Get Child Transactions

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `GET` | `GET` |
| **Route** | `/api/ResponseTransactions/{parentTransactionId}/children` | `/api/RequestTransactions/{parentTransactionId}/children` |
| **Permission** | `response-transactions.get-children` | `request-transactions.get-children` |
| **Parameter** | `Guid parentTransactionId` | `Guid parentTransactionId` |
| **Response** | `List<ResponseTransactionDto>` | `List<RequestTransactionDto>` |

**Example Request**:
```bash
GET /api/RequestTransactions/a1b2c3d4-e5f6-7890-abcd-ef1234567890/children
Authorization: Bearer <token>
```

---

### 4. Get Root Transaction

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `GET` | `GET` |
| **Route** | `/api/ResponseTransactions/root/{responseId}` | `/api/RequestTransactions/root/{requestId}` |
| **Permission** | `response-transactions.get-root` | `request-transactions.get-root` |
| **Parameter** | `Guid responseId` | `Guid requestId` |
| **Response** | `ResponseTransactionDto` | `RequestTransactionDto` |

**Example Request**:
```bash
GET /api/RequestTransactions/root/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer <token>
```

---

### 5. Get Transaction Tree

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `GET` | `GET` |
| **Route** | `/api/ResponseTransactions/{transactionId}/tree` | `/api/RequestTransactions/{transactionId}/tree` |
| **Permission** | `response-transactions.get-tree` | `request-transactions.get-tree` |
| **Parameter** | `Guid transactionId` | `Guid transactionId` |
| **Response** | `List<ResponseTransactionDto>` | `List<RequestTransactionDto>` |

**Example Request**:
```bash
GET /api/RequestTransactions/a1b2c3d4-e5f6-7890-abcd-ef1234567890/tree
Authorization: Bearer <token>
```

---

### 6. Create Initial Transaction

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `POST` | `POST` |
| **Route** | `/api/ResponseTransactions` | `/api/RequestTransactions` |
| **Permission** | `response-transactions.create` | `request-transactions.create` |
| **Content-Type** | `multipart/form-data` | `multipart/form-data` |
| **DTO** | `AddInitialResponseTransactionDto` | `AddInitialRequestTransactionDto` |
| **Request Body** | `{ ResponseId, Notes, CurrentUserHolderId, Files }` | `{ RequestId, Notes, CurrentUserHolderId, Files }` |

**Example Request**:
```bash
POST /api/RequestTransactions
Content-Type: multipart/form-data
Authorization: Bearer <token>

{
  "RequestId": "123e4567-e89b-12d3-a456-426614174000",
  "Notes": "Initial transaction notes",
  "CurrentUserHolderId": "user-guid-here",
  "Files": [file1, file2]
}
```

**Key Changes**:
- DTO property changed from `ResponseId` to `RequestId`
- Request status automatically changes to `InProcess` when first transaction is created

---

### 7. Add Child Transaction

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `POST` | `POST` |
| **Route** | `/api/ResponseTransactions/AddTransactionChildren` | `/api/RequestTransactions/AddTransactionChildren` |
| **Permission** | `response-transactions.add-child` | `request-transactions.add-child` |
| **Content-Type** | `multipart/form-data` | `multipart/form-data` |
| **DTO** | `CreateResponseTransactionDto` | `CreateRequestTransactionDto` |
| **Request Body** | `{ ResponseId, ParentTransactionId, Notes, CurrentUserHolderId, Files }` | `{ RequestId, ParentTransactionId, Notes, CurrentUserHolderId, Files }` |

**Example Request**:
```bash
POST /api/RequestTransactions/AddTransactionChildren
Content-Type: multipart/form-data
Authorization: Bearer <token>

{
  "RequestId": "123e4567-e89b-12d3-a456-426614174000",
  "ParentTransactionId": "parent-transaction-guid",
  "Notes": "Child transaction notes",
  "CurrentUserHolderId": "user-guid-here",
  "Files": [file1]
}
```

**Key Changes**:
- All `ResponseId` references changed to `RequestId`
- `ParentTransactionId` is now part of DTO (not route parameter)

---

### 8. Get Paged Transactions

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `GET` | `GET` |
| **Route** | `/api/ResponseTransactions` | `/api/RequestTransactions` |
| **Permission** | `response-transactions.get-paged` | `request-transactions.get-paged` |
| **Query Parameters** | `page`, `pageSize`, `status` | `page`, `pageSize`, `status` |
| **Response** | `PagedList<ResponseTransactionDto>` | `PagedList<RequestTransactionDto>` |

**Example Requests**:
```bash
# Get all transactions (paged)
GET /api/RequestTransactions?page=1&pageSize=10
Authorization: Bearer <token>

# Filter by status
GET /api/RequestTransactions?page=1&pageSize=10&status=InProgress
Authorization: Bearer <token>
```

**Available Status Values**:
- `PendingAction` (0)
- `Transferred` (1)

---

### 9. Mark Request As Managed

| Aspect | Before | After |
|--------|--------|-------|
| **Method** | `POST` | `POST` |
| **Route** | `/api/ResponseTransactions/{responseId}/mark-as-managed` | `/api/RequestTransactions/{requestId}/mark-as-managed` |
| **Permission** | `response-transactions.mark-as-managed` | `request-transactions.mark-as-managed` |
| **Parameter** | `Guid responseId` | `Guid requestId` |
| **Response** | `ResponseDto` | `RequestDto` |

**Example Request**:
```bash
POST /api/RequestTransactions/123e4567-e89b-12d3-a456-426614174000/mark-as-managed
Authorization: Bearer <token>
```

**Key Changes**:
- Now marks the **Request** as `Managed` status (not Response)
- Returns `RequestDto` instead of `ResponseDto`

---

## Complete Endpoints Reference Table

| # | Method | Route | Permission | Description |
|---|--------|-------|------------|-------------|
| 1 | `GET` | `/api/RequestTransactions/{id}` | `request-transactions.get-by-id` | Get transaction by ID |
| 2 | `GET` | `/api/RequestTransactions/by-request/{requestId}` | `request-transactions.get-by-request-id` | Get all transactions for a request |
| 3 | `GET` | `/api/RequestTransactions/{parentTransactionId}/children` | `request-transactions.get-children` | Get child transactions |
| 4 | `GET` | `/api/RequestTransactions/root/{requestId}` | `request-transactions.get-root` | Get root transaction |
| 5 | `GET` | `/api/RequestTransactions/{transactionId}/tree` | `request-transactions.get-tree` | Get transaction tree hierarchy |
| 6 | `GET` | `/api/RequestTransactions` | `request-transactions.get-paged` | Get paged transactions (with optional status filter) |
| 7 | `POST` | `/api/RequestTransactions` | `request-transactions.create` | Create initial transaction |
| 8 | `POST` | `/api/RequestTransactions/AddTransactionChildren` | `request-transactions.add-child` | Add child transaction |
| 9 | `POST` | `/api/RequestTransactions/{requestId}/mark-as-managed` | `request-transactions.mark-as-managed` | Mark request as managed |

---

## DTO Changes

### AddInitialRequestTransactionDto (previously AddInitialResponseTransactionDto)
```csharp
public class AddInitialRequestTransactionDto
{
    public Guid RequestId { get; set; }           // Changed from ResponseId
    public string Notes { get; set; } = string.Empty;
    public Guid CurrentUserHolderId { get; set; }
    public IFormFileCollection? Files { get; set; }
}
```

### CreateRequestTransactionDto (previously CreateResponseTransactionDto)
```csharp
public class CreateRequestTransactionDto
{
    public Guid RequestId { get; set; }           // Changed from ResponseId
    public string Notes { get; set; } = string.Empty;
    public Guid? ParentTransactionId { get; set; }
    public Guid CurrentUserHolderId { get; set; }
    public IFormFileCollection? Files { get; set; }
}
```

### RequestTransactionDto (previously ResponseTransactionDto)
```csharp
public class RequestTransactionDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }           // Changed from ResponseId
    public TransactionStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Path { get; set; } = string.Empty;
    public Guid? ParentTransactionId { get; set; }
    public Guid CurrentUserHolderId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public RequestTransactionDto? ParentTransaction { get; set; }
    public List<RequestTransactionDto> ChildTransactions { get; set; } = [];
    public List<RequestTransactionAttachmentDto> RequestTransactionAttachments { get; set; } = [];
    public int AttachmentCount => RequestTransactionAttachments.Count;
    public int ChildCount => ChildTransactions.Count;
}
```

---

## Request Status Workflow

The `Request` entity now has a `RequestStatus` property that tracks the workflow:

```
Request Created (Pending)
    ↓
First Transaction Created (InProcess)
    ↓
Transactions Workflow (InProcess)
    ↓
Marked As Managed (Managed)
```

### RequestStatus Enum Values:
| Status | Value | Description |
|--------|-------|-------------|
| `Pending` | 0 | Initial state when request is created |
| `Delivered` | 1 | Request has been delivered/assigned |
| `InProcess` | 2 | Transaction workflow is active |
| `Managed` | 3 | Request workflow completed |

---

## Breaking Changes

### 1. Route Changes
- **Impact**: All client applications using `/api/ResponseTransactions` must update to `/api/RequestTransactions`
- **Severity**: 🔴 **HIGH** - Breaking change

### 2. DTO Property Changes
- **Impact**: `ResponseId` → `RequestId` in all DTOs
- **Severity**: 🔴 **HIGH** - Breaking change

### 3. Permission String Changes
- **Impact**: Role/permission assignments must be updated from `response-transactions.*` to `request-transactions.*`
- **Severity**: 🟡 **MEDIUM** - Requires configuration update

### 4. Response Type Changes
- **Impact**: Endpoints returning `ResponseTransactionDto` now return `RequestTransactionDto`
- **Severity**: 🟡 **MEDIUM** - May require frontend updates

---

## Migration Guide for API Consumers

### Step 1: Update Base URL
```javascript
// Before
const BASE_URL = '/api/ResponseTransactions';

// After
const BASE_URL = '/api/RequestTransactions';
```

### Step 2: Update Request Bodies
```javascript
// Before
{
  "ResponseId": "guid-here",
  "Notes": "notes",
  "CurrentUserHolderId": "user-guid"
}

// After
{
  "RequestId": "guid-here",
  "Notes": "notes",
  "CurrentUserHolderId": "user-guid"
}
```

### Step 3: Update Query Parameters
```javascript
// Before
GET /api/ResponseTransactions/by-response/{responseId}

// After
GET /api/RequestTransactions/by-request/{requestId}
```

### Step 4: Update Permission Checks
```javascript
// Before
if (user.hasPermission('response-transactions.create')) { ... }

// After
if (user.hasPermission('request-transactions.create')) { ... }
```

---

## Testing Checklist

- [ ] Test creating initial transaction for a request
- [ ] Test adding child transactions
- [ ] Test filtering transactions by status
- [ ] Test getting transaction tree
- [ ] Test marking request as managed
- [ ] Verify Request status changes correctly (Pending → InProcess → Managed)
- [ ] Verify authorization with new permission strings
- [ ] Test file uploads with transactions
- [ ] Verify all old endpoints return 404
- [ ] Test pagination with status filter

---

## Related Files Changed

### Domain Layer
- `RequestTransaction.cs` - New entity
- `RequestTransactionAttachment.cs` - New entity
- `RequestTransactionExpressions.cs` - New expressions
- `Request.cs` - Added `Status`, `FirstTransactionId`, `CurrentTransactionId`

### Application Layer
- `IRequestTransactionService.cs` - Service interface
- `AddInitialRequestTransactionDto.cs` - DTO
- `CreateRequestTransactionDto.cs` - DTO
- `RequestTransactionDto.cs` - DTO

### Infrastructure Layer
- `RequestTransactionService.cs` - Service implementation
- `IWebAttachmentService.cs` - Updated method signature
- `WebAttachmentService.cs` - Updated implementation
- `IUnitOfWork.cs` - Updated repository references
- `UnitOfWork.cs` - Updated repository implementations
- `AppDbContext.cs` - Updated relationships

### Presentation Layer
- `RequestTransactionsController.cs` - New controller
- `InfrastructureServiceRegistration.cs` - Updated DI registration

---

## Notes

1. **Database Migration**: A new migration `MoveStatusToRequestAndUpdateRelationships` has been created to handle schema changes
2. **Backward Compatibility**: This is a **breaking change** - old endpoints will not work
3. **Status Location**: Status moved from `Response` entity to `Request` entity
4. **Enum Naming**: `ResponseStatus` renamed to `RequestStatus`
5. **File Attachments**: File upload functionality remains unchanged, only entity references updated

---

**Report Generated**: April 12, 2026
**Total Endpoints Affected**: 9
**Breaking Changes**: Yes (Route, DTO properties, Permissions)

---

## Suggested Commit Message

### Conventional Commit Format

```
refactor: rename ResponseTransaction to RequestTransaction and move status to Request

BREAKING CHANGE: Transactions now belong to Requests instead of Responses

- Rename ResponseTransaction to RequestTransaction across all layers
- Move ResponseStatus enum to Request entity as RequestStatus
- Update all DTOs, services, and controllers
- Change API route from /api/ResponseTransactions to /api/RequestTransactions
- Update permission strings from response-transactions.* to request-transactions.*
- Add FirstTransactionId and CurrentTransactionId to Request entity
- Remove transaction properties from Response entity
- Update AppDbContext relationships and configurations
- Create migration: MoveStatusToRequestAndUpdateRelationships

Business Logic:
- Request (Pending) → First Transaction (InProcess) → Managed
- Transactions are part of Request workflow, not Response
- Response is the final outcome, not the transaction container

Files Changed:
- Domain: RequestTransaction, RequestTransactionAttachment, RequestTransactionExpressions
- Application: IRequestTransactionService, DTOs
- Infrastructure: RequestTransactionService, UnitOfWork, WebAttachmentService
- Presentation: RequestTransactionsController
- Database: New migration created

Migration Required: MoveStatusToRequestAndUpdateRelationships
```

### Short Version (One-Liner)

```
refactor: rename ResponseTransaction to RequestTransaction, move status to Request (BREAKING CHANGE)
```

### Detailed Version with All Changes

```
refactor: rename ResponseTransaction to RequestTransaction and move status to Request

BREAKING CHANGE: Transactions now belong to Requests instead of Responses

Summary:
This refactoring corrects the business logic by moving transactions from the Response
entity to the Request entity, where they logically belong. The Request goes through a
workflow (Pending → InProcess → Managed) with transactions representing the workflow steps,
while the Response is simply the final outcome.

Entity Changes:
✓ RequestTransaction (new) - replaces ResponseTransaction
✓ RequestTransactionAttachment (new) - replaces ResponseTransactionAttachment
✓ RequestTransactionExpressions (new) - replaces ResponseTransactionExpressions
✓ Request.cs - Added Status, FirstTransactionId, CurrentTransactionId
✓ Response.cs - Removed Status and transaction relationships

Service Layer:
✓ IRequestTransactionService interface
✓ RequestTransactionService implementation
✓ Updated IWebAttachmentService and WebAttachmentService
✓ Removed ResponseService status change logic

API Endpoints:
✓ RequestTransactionsController (new) - replaces ResponseTransactionsController
✓ All routes changed: /api/ResponseTransactions → /api/RequestTransactions
✓ All permissions changed: response-transactions.* → request-transactions.*
✓ DTOs updated: ResponseId → RequestId

Database:
✓ Removed migrations: AddStatusToResponseAndTransactionRelationships, AddTransactionStatusToResponseTransaction
✓ Created migration: MoveStatusToRequestAndUpdateRelationships

Testing:
✓ Build successful - no compilation errors
✓ All references updated
✓ Migration created and verified

Breaking Changes for API Consumers:
1. Base route changed: /api/ResponseTransactions → /api/RequestTransactions
2. All DTO properties: ResponseId → RequestId
3. Permission strings updated
4. Response types changed from ResponseTransactionDto to RequestTransactionDto
```

### How to Use

**Option 1: Standard Commit**
```bash
git add .
git commit -m "refactor: rename ResponseTransaction to RequestTransaction and move status to Request

BREAKING CHANGE: Transactions now belong to Requests instead of Responses

- Rename ResponseTransaction to RequestTransaction across all layers
- Move ResponseStatus enum to Request entity as RequestStatus
- Update all DTOs, services, and controllers
- Change API route from /api/ResponseTransactions to /api/RequestTransactions
- Update permission strings from response-transactions.* to request-transactions.*
- Add FirstTransactionId and CurrentTransactionId to Request entity
- Remove transaction properties from Response entity
- Update AppDbContext relationships and configurations
- Create migration: MoveStatusToRequestAndUpdateRelationships

Migration Required: MoveStatusToRequestAndUpdateRelationships"
```

**Option 2: Using Commit Message File**
```bash
# Save the detailed version to a file
git add .
git commit -F COMMIT_MSG.txt
```

---

## Git Commands to Execute

### Stage All Changes
```bash
git add -A
```

### Verify Staged Changes
```bash
git status
git diff --staged --stat
```

### Commit with Message
```bash
git commit -m "refactor: rename ResponseTransaction to RequestTransaction and move status to Request

BREAKING CHANGE: Transactions now belong to Requests instead of Responses

Entity Changes:
- RequestTransaction replaces ResponseTransaction
- RequestTransactionAttachment replaces ResponseTransactionAttachment
- RequestTransactionExpressions replaces ResponseTransactionExpressions
- Request entity: Added Status, FirstTransactionId, CurrentTransactionId
- Response entity: Removed Status and transaction relationships

Service Layer:
- IRequestTransactionService interface created
- RequestTransactionService implementation created
- Updated IWebAttachmentService and WebAttachmentService
- Removed ResponseService status change logic

API Endpoints:
- RequestTransactionsController replaces ResponseTransactionsController
- Routes: /api/ResponseTransactions → /api/RequestTransactions
- Permissions: response-transactions.* → request-transactions.*
- DTOs: ResponseId → RequestId

Database:
- Migration created: MoveStatusToRequestAndUpdateRelationships

Build Status: ✓ Successful"
```

### Verify Commit
```bash
git log -1 --stat
git show --stat
```

### Push to Remote (When Ready)
```bash
git push origin features/addResponseTransaction
```

---

## Commit Message Convention Used

This commit follows [Conventional Commits](https://www.conventionalcommits.org/) specification:

- **Type**: `refactor` - Code change that neither fixes a bug nor adds a feature
- **Scope**: Implicit in the description (ResponseTransaction → RequestTransaction)
- **Subject**: Clear, concise description of the change
- **Body**: Detailed explanation of what changed and why
- **BREAKING CHANGE**: Footer indicating this is a breaking change
- **Migration Note**: Database migration required

---

## Post-Commit Checklist

- [ ] Verify commit was created successfully
- [ ] Check commit message is clear and complete
- [ ] Review changed files with `git diff --staged`
- [ ] Test build after commit
- [ ] Push to remote branch when ready
- [ ] Notify team members of breaking changes
- [ ] Update API documentation
- [ ] Update frontend code to use new endpoints
- [ ] Test database migration in staging environment

