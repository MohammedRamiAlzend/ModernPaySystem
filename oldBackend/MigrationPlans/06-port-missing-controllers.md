---
tags: [migration, controllers, archive]
module: ArchiveSystem
status: draft
priority: high
depends-on: [02-fix-archive-services]
---

# 06 — Port Missing Archive Controllers

## Problem

**4 controllers** from the old archiving module have no equivalent in the new `src/` structure. These represent real missing functionality.

## Controllers to Create

### 1. DynamicFormsController → `Archive.Api/Controllers/DynamicFormsController.cs`

**Old**: `ArchivingControllers/DynamicFormsController.cs` (6 endpoints)
- `GET /` — getAll
- `GET /paged` — getPaged
- `GET /{id}` — getById
- `GET /by-name/{name}` — getByName
- `POST /` — create
- `PUT /{id}` — update
- `DELETE /{id}` — delete

**Interface (exists)**: `IArchiveFormTemplateService` in `Archive.Application/Interfaces/`
**Route**: `api/archive/forms`

### 2. FolderIconsController → `Archive.Api/Controllers/FolderIconsController.cs`

**Old**: `ArchivingControllers/FolderIconsController.cs` (7 endpoints)
- `GET /`, `GET /{id}`, `GET /{id}/svg`, `POST /`, `PUT /{id}`, `DELETE /{id}`, `POST /assign`

**Interface (missing)**: Need to create `IFolderIconService` in `Archive.Application/Interfaces/`
**Implementation (missing)**: Need to create `FolderIconService` in `Archive.Infrastructure/Services/`
**Route**: `api/archive/folder-icons`

### 3. FoldersController → `Archive.Api/Controllers/FoldersController.cs`

**Old**: `ArchivingControllers/FoldersController.cs` (11 endpoints)
- CRUD + Move + permissions CRUD

**Interface (missing)**: Need to create `IFolderService` in `Archive.Application/Interfaces/`
**Implementation (missing)**: Need to create `FolderService` in `Archive.Infrastructure/Services/`
**Route**: `api/archive/folders`

### 4. DocumentIndexingController → (deferred — see [[10-port-ocr-and-semantic-search]])

This controller depends on `ISemanticSearchService` which has no equivalent in the new structure. Defer until semantic search is ported.

## Work Estimates

| Controller | Interface Exists? | Service Exists? | Effort |
|---|---|---|---|
| `DynamicFormsController` | ✅ Yes | ❌ No | 3h |
| `FolderIconsController` | ❌ Create | ❌ Create | 4h |
| `FoldersController` | ❌ Create | ❌ Create | 8h |
| `DocumentIndexingController` | ❌ Deferred | ❌ Deferred | — |

## Verification

```bash
dotnet build src/ModernPaySystem.Boot/ModernPaySystem.Boot.csproj
# Then curl each new endpoint
```
