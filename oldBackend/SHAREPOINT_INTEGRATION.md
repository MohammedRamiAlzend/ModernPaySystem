# SharePoint Integration for ModernPaySystem Archive

## What is SharePoint?

Microsoft SharePoint is a cloud-based document management and collaboration platform. It provides:

- **Document Libraries** — structured folders/files with metadata, versioning, check-in/check-out
- **Graph API** — RESTful access to files, lists, sites via `https://graph.microsoft.com`
- **Permissions** — granular access control integrated with Entra ID
- **Sync & Office Integration** — native editing in Word/Excel/PPT, OneDrive sync
- **Search** — full-text and metadata search across all sites and libraries

## Why Integrate SharePoint with the Archive System?

| Need | SharePoint Benefit |
|---|---|
| **Centralized storage** | Move files out of the app server's local disk into a managed cloud repository |
| **Disaster recovery** | SharePoint files are backed up by Microsoft; no separate backup pipeline needed |
| **Scalability** | No disk space worries — SharePoint handles storage limits at the tenant level |
| **External sharing** | Securely share archive documents with external auditors or partners via SharePoint sharing links |
| **Compliance** | Built-in retention policies, eDiscovery, and audit logging |
| **Familiar UI** | Users can access archive files directly from their SharePoint/OneDrive if needed |
| **Access control** | Use existing Entra ID groups for archive folder permissions instead of a custom system |

## Current Architecture

```
┌────────────────────────────────────────────────────────────┐
│                    ArchiveRecordService                     │
│  (CRUD, file upload/download, ZIP, pagination, audit)      │
└──────────────┬─────────────────────────────────────────────┘
               │  uses
     ┌─────────┴──────────┐
     ▼                    ▼
IFileManager        IFilesManagerService
(low-level FS ops)  (web-focused wrapper)
     │                    │
     └────────┬───────────┘
              ▼
    EnhancedFileManager
    (System.IO — local disk)
```

All file I/O goes through `IFileManager` (read/write/copy/move/delete/list directories) and `IFilesManagerService` (save `IFormFile`, get bytes/stream, check existence). Because these are interfaces, you can swap the implementation without touching any business logic.

## Implementation Strategy

### Approach: Replace the File Manager Implementation

Add a new project or folder `ModernPaySystem.Infrastructure.FileStorage.SharePoint/` containing:

```
ModernPaySystem.Infrastructure.FileStorage.SharePoint/
├── SharePointOptions.cs              # Connection config (site, tenant, clientId, etc.)
├── SharePointFileManager.cs          # IFileManager implementation
├── SharePointFilesManagerService.cs  # IFilesManagerService implementation
├── ServiceCollectionExtensions.cs    # DI registration
```

### IFileManager → SharePoint Mapping

| IFileManager Method | SharePoint Graph API (Microsoft.Graph SDK) |
|---|---|
| `RootDirectory` | Returns the SharePoint site URL |
| `WriteFileAsync(path, bytes)` | `PUT /sites/{site-id}/drive/root:/{path}:/content` |
| `ReadFileAsync(path)` | `GET /sites/{site-id}/drive/root:/{path}` then `GET /drive/items/{id}/content` |
| `DeleteFileAsync(path)` | `DELETE /sites/{site-id}/drive/items/{id}` |
| `FileExists(path)` | `GET /sites/{site-id}/drive/root:/{path}` — 200 vs ServiceException(404) |
| `CreateDirectoryAsync(path)` | `PATCH /sites/{site-id}/drive/root:/{parentPath}/{name}:/` with `{ "folder": {} }` |
| `DirectoryExists(path)` | Same as FileExists on a folder-type driveItem |
| `ListDirectoryAsync(path)` | `GET /sites/{site-id}/drive/root:/{path}:/children` |
| `MoveFileAsync(src, dst)` | `PATCH /sites/{site-id}/drive/items/{id}` with new `parentReference` |
| `CopyFileAsync(src, dst)` | `POST /sites/{site-id}/drive/items/{id}/copy` |
| `RenameFileAsync(path, name)` | `PATCH /sites/{site-id}/drive/items/{id}` with `{ "name": "newName" }` |
| `GetFileInfoAsync(path)` | `GET /sites/{site-id}/drive/root:/{path}` → parse `size`, `lastModifiedDateTime`, `createdDateTime` |
| `SearchFilesAsync(path, criteria)` | `GET /sites/{site-id}/drive/root/search(q='{query}')` |
| `GenerateSafeFileName(name)` | Pure logic — no SharePoint call needed; keep existing method |

### IFilesManagerService Mapping

| IFilesManagerService Method | Implementation |
|---|---|
| `SaveFileAsync(IFormFile, subDir)` | Read IFormFile stream → `SharePointFileManager.WriteFileAsync` with path `Diwan/Uploads/{subDir}/{safeFileName}` |
| `GetFileBytesAsync(path)` | `SharePointFileManager.ReadFileAsync(path)` |
| `GetFileStreamAsync(path)` | Download file bytes and wrap in `MemoryStream` (Graph API doesn't support remote streams easily; buffer into memory) |
| `DeleteFileAsync(path)` | `SharePointFileManager.DeleteFileAsync(path)` |
| `FileExists(path)` | `SharePointFileManager.FileExists(path)` |
| `GetFileInfo(path)` | `SharePointFileManager.GetFileInfoAsync(path)` → map to `FileMetadata` |
| `GetContentType(ext)` | Pure logic — no SharePoint call needed |
| `GetFileTypeFromExtension(ext)` | Pure logic — no SharePoint call needed |
| `IsValidFileExtension(ext, allowed)` | Pure logic — no SharePoint call needed |
| `GenerateSafeFileName(name)` | Pure logic — no SharePoint call needed |
| `CleanupOldFilesAsync(days)` | List children recursively, filter by `lastModifiedDateTime < threshold`, delete each |
| `UploadsDirectory` | Returns `"Diwan/Uploads"` (used as root relative path in SharePoint) |

### Authentication Setup (Entra ID)

1. Register an app in [Microsoft Entra ID](https://entra.microsoft.com)
2. Grant API permission: `Sites.ReadWrite.All` (Application type)
3. Create a client secret
4. Use `ClientSecretCredential` from `Azure.Identity`:

```csharp
var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
var graphClient = new GraphServiceClient(credential);
```

### DI Registration

In `InfrastructureServiceRegistration.cs`, add a conditional registration:

```csharp
var sharePointEnabled = configuration.GetValue<bool>("SharePoint:Enabled");
if (sharePointEnabled)
{
    services.Configure<SharePointOptions>(configuration.GetSection("SharePoint"));
    services.AddSingleton<GraphServiceClient>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<SharePointOptions>>().Value;
        var credential = new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);
        return new GraphServiceClient(credential);
    });
    services.AddSingleton<IFileManager, SharePointFileManager>();
    services.AddScoped<IFilesManagerService, SharePointFilesManagerService>();
}
else
{
    // keep existing local registration
}
```

### Configuration (appsettings.json)

```json
{
  "SharePoint": {
    "Enabled": false,
    "SiteUrl": "https://{tenant}.sharepoint.com/sites/{site-name}",
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": ""
  }
}
```

### Storage Path Mapping

Current local paths like:
```
Diwan/Uploads/{subDirectory}/{recordId}/{safeFileName}
```

Become SharePoint drive paths:
```
/Diwan/Uploads/{subDirectory}/{recordId}/{safeFileName}
```

The `SharePointFileManager` interprets all paths as relative to the drive root. The `RootDirectory` property returns the site URL for reference but isn't used in path resolution.

### Considerations & Caveats

| Concern | Mitigation |
|---|---|
| **Latency** | SharePoint API calls are slower than local disk. Cache file streams in `IMemoryCache` (already used for ZIP bundles). |
| **Rate limiting** | Graph API has throttling (10k requests per 10s per app). Implement retry with exponential backoff via `Polly`. |
| **Streaming** | Graph API returns file content as an `HttpResponseMessage` stream. Use `ReadAsStreamAsync()` instead of buffering entire files for large files. |
| **Search** | SharePoint's `search(q)` is less flexible than `FileSearchCriteria`. Consider doing client-side filtering after retrieving children. |
| **Cost** | SharePoint storage consumes your tenant's pool. Monitor usage. |
| **Offline** | If SharePoint is unreachable, the archive system fails. Add a health-check endpoint and circuit breaker pattern. |

## Testing the Integration

- Run the existing `dotnet test` suite — all `ArchiveRecordService` tests should pass unchanged since `IFileManager`/`IFilesManagerService` are mocked in unit tests
- For integration tests, create a test SharePoint site and run file upload/download/delete workflows against it
- Use the existing HTTP workflow files in `ModernPaySystem/HttpWorkFlows/03-archive-records-workflow.http` to manually test the API after configuring SharePoint

## Rollout Plan

1. **Phase 1** — Implement `SharePointFileManager` with the core methods (Write, Read, Delete, FileExists, ListDirectory)
2. **Phase 2** — Implement `SharePointFilesManagerService` and wire DI
3. **Phase 3** — Enable in staging environment, run smoke tests
4. **Phase 4** — Migrate existing files from local disk to SharePoint (copy via background job)
5. **Phase 5** — Flip the `Enabled` flag in production
