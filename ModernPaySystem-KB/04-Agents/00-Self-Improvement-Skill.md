# Self-Improvement Skill — ModernPaySystem-KB

## Purpose

This skill defines how the Knowledge Base must be **automatically updated** whenever the codebase changes. It ensures the KB stays synchronized with the actual solution architecture, patterns, and conventions. **No code change is complete until the KB is updated.**

---

## Trigger Conditions

Update the KB whenever any of the following events occur:

| Event | Required KB Updates |
|-------|---------------------|
| **New Domain entity created** | `03-Projects/03-ModernPaySystem-Domain/domain-skill.md` (add entity to folder list, add ToDto pattern if needed) |
| **New DTO created** | `03-Projects/03-ModernPaySystem-Domain/domain-skill.md`, `03-Projects/02-ModernPaySystem-Application/application-skill.md` (interface signatures) |
| **New service interface added** | `03-Projects/02-ModernPaySystem-Application/application-skill.md` (add to interface list) |
| **New service implementation added** | Update `03-Projects/04-ModernPaySystem-Infrastructure/infrastructure-skill.md`, `ModernPaySystem.Infrastructure/InfrastructureServiceRegistration.cs` reference check |
| **New controller added** | `03-Projects/01-ModernPaySystem/api-skill.md` (add to controller list), folder naming convention check |
| **New repository property in IUnitOfWork** | `03-Projects/05-ModernPaySystem-Infrastructure-Persistence/persistence-skill.md`, `03-Projects/03-ModernPaySystem-Domain/domain-skill.md` (Rule 9 checklist) |
| **New DbSet in AppDbContext** | `03-Projects/05-ModernPaySystem-Infrastructure-Persistence/persistence-skill.md`, `IUnitOfWork.cs` |
| **New library project added** | `03-Projects/0X-{LibraryName}/library-skill.md`, `vault-structure.md`, `dependencies` check |
| **Entity moved between namespaces/folders** | `domain-skill.md` (folder structure), `IUnitOfWork.cs`, any referencing interfaces/services/controllers |
| **New .csproj package reference added** | `07-Dependencies/third-party-libs.md` |
| **New middleware added** | `03-Projects/01-ModernPaySystem/api-skill.md` |
| **New redirect or rewrite rule** | `vault-structure.md`, `03-Projects/01-ModernPaySystem/api-skill.md` |

---

## Self-Update Protocol

When an event triggers a KB update, follow this protocol:

### Step 1: Identify Changed Files

```markdown
# Run after any code change:
git diff --name-only
git status --short
```

### Step 2: Map Changes to KB Files

Use the trigger table above to determine which KB files need updating.

### Step 3: Update KB Files

For each affected KB file:

1. **Read the current KB file**
2. **Add the new information** in the appropriate section:
   - New entity → add to Domain folder structure table
   - New service → add to Application interfaces list
   - New controller → add to API controller list
   - New entity registration → add to persistence checker list, domain checklist
3. **Maintain consistency** — if adding a new entity to Domain, ensure it appears in:
   - `domain-skill.md` entity examples
   - `IUnitOfWork.cs` (and mention in KB)
   - `persistence-skill.md` registration rules
   - `infrastructure-skill.md` service patterns (if it has a service)

### Step 4: Verify KB Health

```markdown
# Quick checks:
1. Does vault-structure.md list all folders?
2. Does dependency-rules.md reflect new package references?
3. Do all folder structure tables match actual filesystem?
4. Are all new entities listed in domain-skill.md?
5. Is IUnitOfWork property documented for new entities?
```

### Step 5: Commit KB Changes

KB updates should be committed alongside code changes.

```
git add ModernPaySystem-KB/
git commit -m "docs: update KB for new {entity/service/controller} {Name}"
```

---

## Per-Project Update Rules

### When Adding a New Entity

1. **Domain** → `domain-skill.md`:
   - Add to folder structure tree
   - Add to entity naming conventions table
   - Add entity creation example if it introduces a new pattern

2. **Persistence** → `persistence-skill.md`:
   - Create entity in `Domain/Entities/{Category}/{Entity}.cs`
   - Add `DbSet<Entity>` to `AppDbContext`
   - Add `IRepositoryBase<Entity, Guid> Entities { get; }` to `IUnitOfWork`
   - Add property implementation to `UnitOfWork.cs`
   - Mark checklist item: `- [ ] Entity registered in IUnitOfWork...`

3. **Application** → `application-skill.md`:
   - Add interface to `Interfaces/I{Entity}Service.cs` if service exists
   - Add DTOs to `Domain/DTOs/{Category}/`

4. **Infrastructure** → `infrastructure-skill.md`:
   - Add service implementation to `Services/{Entity}Service.cs`
   - Add DI registration in `InfrastructureServiceRegistration.cs`
   - Update service list in KB

### When Adding a New Service Interface

1. **Application** → `application-skill.md`:
   - Add `I{Entity}Service` to interfaces list
   - Document method signatures if introducing new pattern

2. **Infrastructure** → `infrastructure-skill.md`:
   - Add `{Entity}Service` to implementations list
   - Show constructor pattern (inject IUnitOfWork)

### When Adding a New Controller

1. **API** → `api-skill.md`:
   - Add controller to list
   - Document route pattern if non-standard
   - Document permission attribute pattern if used

### When Adding a New Library

1. **KB Root** → `vault-structure.md`:
   - Add new `03-Projects/0X-{LibraryName}/` entry
   - Create `library-skill.md`

2. **Library-specific** → `03-Projects/0X-{LibraryName}/library-skill.md`:
   - Use library-skill-template.md as base
   - Document public API, usage patterns, extension points

---

## AI Coding Rule — KB Completeness

```
Before considering ANY task complete:
1. Check if new entity/service/controller/library was added
2. If yes → update corresponding KB files
3. Run through Trigger Conditions table to find all affected KB files
4. Update each affected KB file with new information
5. Verify vault-structure.md reflects actual folder layout
```

### Quick Checklist

```markdown
- [ ] New entity → updated domain-skill.md (folder structure, examples)
- [ ] New entity → added to IUnitOfWork (mentioned in persistence-skill.md)
- [ ] New service → listed in application-skill.md and infrastructure-skill.md
- [ ] New controller → listed in api-skill.md
- [ ] New library → vault-structure.md + library-skill.md created
- [ ] New package reference → added to dependencies (if significant)
- [ ] Any namespace/folder changes → reflected in all affected skill files
```

---

## Example: Adding a New Entity "Invoice"

**Code changes:**
1. `Domain/Entities/TransactionSystemEntities/Invoice.cs` created
2. `AppDbContext.cs` → `DbSet<Invoice> Invoices { get; set; }`
3. `IUnitOfWork.cs` → `IRepositoryBase<Invoice, Guid> Invoices { get; }`
4. `UnitOfWork.cs` → property implementation
5. `Infrastructure/Services/InvoiceService.cs` created
6. `Application/Interfaces/IInvoiceService.cs` created
7. `Controllers/TransactionsSystemControllers/InvoicesController.cs` created

**KB changes required:**

| File | Update |
|------|--------|
| `domain-skill.md` | Add `Invoice.cs` to folder tree, add naming example |
| `persistence-skill.md` | Mention new entity in UnitOfWork section (implicit via checklist) |
| `application-skill.md` | Add `IInvoiceService` to interfaces list |
| `infrastructure-skill.md` | Add `InvoiceService` to implementations list |
| `api-skill.md` | Add `InvoicesController` to controller list |
| `vault-structure.md` | Verify `TransactionsSystemEntities` folder listed |

---

## Example: Adding PostgreSQL Npgsql Package

**Code change:**
- `ModernPaySystem.Infrastructure.Persistence.csproj` adds `Npgsql.EntityFrameworkCore.PostgreSQL`

**KB changes required:**

| File | Update |
|------|--------|
| `01-Architecture/clean-architecture.md` | Database Technology section (already has Npgsql — verify) |
| `01-Architecture/dependency-rules.md` | NuGet package rules table (already has Npgsql — verify) |
| `03-Projects/05-ModernPaySystem-Infrastructure-Persistence/persistence-skill.md` | Already mentions Npgsql — verify accuracy |
| `07-Dependencies/third-party-libs.md` | Add Npgsql entry if not present |

---

## Philosophy

> **"The KB is not documentation — it is the source of truth."**

If the KB says one thing and the code does another, the code is wrong OR the KB must be updated immediately. There is no third option.

When in doubt, **update the KB first**, then write the code to match.
