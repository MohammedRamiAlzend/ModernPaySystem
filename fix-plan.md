# Fix Plan — Archive Deletion Workflow Review

## Critical

### 1. Orphaned records on direct folder soft-delete
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveDeletionWorkflowService.cs:721`

```csharp
// Current: only cascades when requestId.HasValue
if (requestId.HasValue)  // BUG: skips when direct leader deletion
```

**Fix:** Remove the `if (requestId.HasValue)` guard so child `ArchiveRecords` are always soft-deleted when a folder is soft-deleted, regardless of whether it's via approval workflow or direct leader action.

---

### 2. Sync-over-async deadlock in `CanAssignParent`
**File:** `ModernPaySystem.Infrastructure/Services/DepartmentService.cs:503`

```csharp
var result = unitOfWork.Departments.GetByIdAsync(currentId).GetAwaiter().GetResult();
```

**Fix:** Change `CanAssignParent` signature to `async Task<bool>` and `await` the call. Update all callers to await it.

---

## High

### 3. TOCTOU race condition — archival number uniqueness
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveRecordService.cs:220-226`

Uniqueness check happens *before* the transaction begins. A concurrent request can insert the same archival number between check and insert.

**Fix:** Move the `IsArchivalNumberUniqueAsync` check inside the execution strategy transaction, or add a unique database index on `(ArchivalNumber, FolderId)` and handle the constraint violation exception.

---

### 4. `MoveFolderAsync` archival number check outside transaction
**File:** `ModernPaySystem.Infrastructure/Services/FolderService.cs:211-239`

`AreArchiveNumbersUniqueBetweenFolderTrees` checked before update, but the update is not wrapped in a transaction.

**Fix:** Wrap the entire move operation (check + update) inside `unitOfWork.BeginTransactionAsync()` / `CommitTransactionAsync()`.

---

### 5. Error silently swallowed as `false`
**File:** `ModernPaySystem.Infrastructure/Services/FolderService.cs:289-304`

`GetArchiveNumbersInFolderTreeAsync` returns `null` on DB error, which `AreArchiveNumbersUniqueBetweenFolderTrees` treats as "not unique" (blocking the move) and swallows the real error.

**Fix:** Change return type to `Result<HashSet<string>>` so errors propagate properly. Update callers to handle `IsError`.

---

### 6. `Result<T>` returns `null!`
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveRecordService.cs:414`

```csharp
return null!; // Suppresses NRE warning, but fragile
```

**Fix:** Return `Result.Success<ArchiveFormTemplate?>(null)` instead.

---

### 7. `RowVersion` initialized as `[]`
**File:** `ModernPaySystem.Domain/Entities/Archiving/ArchiveGovernance.cs:74`

```csharp
public byte[] RowVersion { get; set; } = [];
```

EF Core generates this server-side via `.IsRowVersion()`. The empty array can corrupt concurrency checks on detached/AsNoTracking entities.

**Fix:** Remove the initializer — leave it as `public byte[] RowVersion { get; set; } = null!;` since EF will always set it.

---

## Medium

### 8. `DestnationFolderId` typo
**File:** `ModernPaySystem.Domain/Entities/Archiving/Folder.cs:87`

```csharp
public Guid DestnationFolderId { get; set; }  // → DestinationFolderId
```

**Fix:** Rename to `DestinationFolderId`. Update all references (controller, service, tests).

---

### 9. Missing `Approved` status in approve/reject guard
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveDeletionWorkflowService.cs:230, 329`

```csharp
// Current guard — missing Approved
if (request.Status is Rejected or Executed)
```

**Fix:** Add `DeleteArchiveRequestStatus.Approved` to the guard clause to prevent double-approval.

---

### 10. `PhysicalFile` missing global query filter
**File:** `ModernPaySystem.Infrastructure.Persistence/AppDbContext.cs`

All other soft-delete entities have `HasQueryFilter(x => !x.IsDeleted)` — `PhysicalFile` does not.

**Fix:** Add `modelBuilder.Entity<PhysicalFile>().HasQueryFilter(pf => !pf.IsDeleted);` to `AppDbContext`.

---

### 11. `ActivitySnapshotJson` never updated after approve/reject
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveDeletionWorkflowService.cs:199-361`

The activity snapshot only captures initial request submission.

**Fix:** Append approval/rejection events to `ActivitySnapshotJson` during `ApproveAsync` / `RejectAsync`.

---

### 12. Wrong error code for duplicate record pending request
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveDeletionWorkflowService.cs:91`

```csharp
return ApplicationErrors.DeleteRequestAlreadyHandled;  // "already processed"
```

**Fix:** Return a new error (e.g., `ArchiveRecordDeleteRequestExists`) indicating a pending request already exists.

---

## Low / Quality

### 13. `ExecutedByUserId` explicitly set to `null`
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveDeletionWorkflowService.cs:263`

```csharp
request.ExecutedByUserId = null;  // Loses audit identity
```

**Fix:** Set `request.ExecutedByUserId = currentUserId` instead, so the approver is recorded as the executor.

---

### 14. `QueryLocks` ConcurrentDictionary memory leak
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveRecordService.cs:34`

```csharp
private static readonly ConcurrentDictionary<string, SemaphoreSlim> QueryLocks = new();
```

Entries are never removed, growing unboundedly.

**Fix:** Use a bounded cache (e.g., `MemoryCache` with sliding expiration) or implement `TryRemove` after use.

---

### 15. Missing `ConfigureAwait(false)` in library code
**File:** All infrastructure services

Per `AGENTS.md` guidelines: "Use `ConfigureAwait(false)` for library code."

**Fix:** Add `.ConfigureAwait(false)` to all `await` calls in `ModernPaySystem.Infrastructure` and `ModernPaySystem.Infrastructure.Persistence`.

---

### 16. Stale entity re-fetch in recursive deletion
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveDeletionWorkflowService.cs:721-727`

`SoftDeleteFolderRecursiveAsync` already loads `ArchiveRecords` with `.Include()`, but calls `SoftDeleteRecordAsync(record.Id, ...)` which re-fetches the same record from DB.

**Fix:** Call the overload `SoftDeleteRecordAsync(record, requestId, ...)` that operates on the already-loaded entity.

---

### 17. Misleading error when `requestId == Guid.Empty`
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveDeletionWorkflowService.cs:302-304`

```csharp
if (requestId == Guid.Empty || string.IsNullOrWhiteSpace(reason))
    return ApplicationErrors.DeleteRequestRejectionRequiresReason;
```

Two different failures share one error message.

**Fix:** Split into two checks with distinct error codes.

---

### 18. Path traversal risk in `Path.GetFullPath`
**File:** `ModernPaySystem.Infrastructure/Services/ArchiveRecordService.cs:1070`

```csharp
var absolutePath = Path.GetFullPath(physicalFile.StoragePath);
```

**Fix:** Validate the resolved path stays within the configured upload root directory before access.

---

## Prioritization

| Priority | Items | Effort |
|----------|-------|--------|
| **P0 — Fix now** | 1, 2 | ~30 min |
| **P1 — Fix this batch** | 3, 4, 5, 6, 7 | ~1 hr |
| **P2 — Next batch** | 8, 9, 10, 11, 12 | ~1.5 hr |
| **P3 — Polish** | 13, 14, 15, 16, 17, 18 | ~2 hr |
