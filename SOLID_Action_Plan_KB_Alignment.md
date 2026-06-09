# SOLID Action Plan × Knowledge Base — Alignment Report

> **Date:** 2026-06-09  
> **Purpose:** Cross-reference every action in `SOLID_Action_Plan.md` against the `ModernPaySystem-KB/` to ensure compliance, identify contradictions, and flag KB gaps.

---

## 1. Alignment Summary

| Status | Count | Meaning |
|--------|-------|---------|
| ✅ Aligned | 6 | Action matches KB rules exactly |
| ⚠️ Needs Update | 3 | Action is correct but KB needs updating |
| ❌ Contradicts KB | 1 | Action violates a documented KB rule |
| 🔴 Blocks Refactor | 2 | KB gap blocks the proposed change |
| 📝 KB Gap Found | 4 | KB is missing content that should exist |

---

## 2. Sprint-by-Sprint Alignment

### Sprint 1 — 🐛 Critical Bug Fixes

| Action | KB Check | Verdict |
|--------|----------|---------|
| **1.1** Remove `filters.AddRange(filters)` in `ResponseService.cs:112` | No KB rule about this specific bug. **KB Gap:** `05-Shared-Knowledge/error-handling.md` is referenced in `vault-structure.md` but the file doesn't exist on disk. | ⚠️ Fix is correct. KB needs `error-handling.md` to document this anti-pattern. |
| **1.2** Fix `DepartmentService.CanAssignParent` sync-over-async | `04-Agents/01-Backend-Agent/agent-skill.md:85` says: *"Always `async Task`, never `.Result` or `.Wait()`"* | ✅ Aligned |
| **1.3** Fix `AttachmentService.IsAttachmentUsedElsewhere` raw `Exception` throw | `04-Agents/01-Backend-Agent/agent-skill.md:84` says: *"`Result<T>` for all service returns, never throw for expected failures"* | ✅ Aligned |

**KB Discovery:** `05-Shared-Knowledge/error-handling.md`, `05-Shared-Knowledge/pagination.md`, `05-Shared-Knowledge/query-building.md` are listed in `vault-structure.md` but don't exist on disk.

---

### Sprint 2 — 🚀 Performance (N+1 + In-Memory)

| Action | KB Check | Verdict |
|--------|----------|---------|
| **2.1** Add `GetAllByIdsAsync` to `IRepositoryBase<T>` and `RepositoryBase<T>` | `01-Architecture/dependency-rules.md:254` — *"Repository interfaces → owned by Application"* and *"Repository implementations → owned by Persistence"*. Adding a method to the generic repository is correct, but it requires updates in 3 places: `IRepositoryBase` (Application), `RepositoryBase` (Persistence), and the KB itself. | ✅ Aligned. Must update: `Application/Repos/IRepositoryBaseT.cs` + `Persistence/Repos/RepositoryBaseT.cs` + KB `application-skill.md:99-140` + `persistence-skill.md` |
| **2.2** Fix in-memory paging in `AttachmentService`, `DepartmentService.SearchAsync` | `05-Shared-Knowledge/query-building.md` is supposed to document dynamic filtering but file is missing. **Blocking:** Without the query-building doc, the pattern for pushing filters down to EF Core isn't documented. | 🔴 Blocks — need to create `query-building.md` first |

---

### Sprint 3 — 🧹 Remove Query Duplication

| Action | KB Check | Verdict |
|--------|----------|---------|
| **3.1** Create `Specifications/RequestIncludes.cs` with extension methods | `01-Architecture/dependency-rules.md:255` — Infrastructure owns service implementations. Extension methods for `IQueryable<T>` belong in Infrastructure. However, the KB says nothing about a `Specifications/` folder pattern. | ⚠️ Aligned in principle. Need to add `Specifications/` to KB's Infrastructure folder structure in `infrastructure-skill.md` and `vault-structure.md` |

---

### Sprint 4 — 🔪 Split Oversized Services

| Action | KB Check | Verdict |
|--------|----------|---------|
| **4.1** Split `ArchiveRecordService` → 3 services | `04-Agents/01-Backend-Agent/agent-skill.md:121` — *"Large service (>7 methods) → Split by concern"* | ✅ Aligned |
| **4.2** Split `RequestService` → extract `RequestRelationService` | Same rule + `01-Architecture/clean-architecture.md:41` — Infrastructure services implement Application interfaces. New interfaces `IArchiveFileService`, `IArchiveTemplateService`, `IRequestRelationService` must be added to `Application/Interfaces/` and registered in `InfrastructureServiceRegistration.cs`. | ✅ Aligned. KB updates needed: `application-skill.md` interface list, `infrastructure-skill.md` DI registration, `vault-structure.md` if new files added. |

---

### Sprint 5 — 🏗️ Segregate God Interfaces

| Action | KB Check | Verdict |
|--------|----------|---------|
| **5.1** Split `IUnitOfWork` into domain-specific sub-interfaces | **❌ THIS CONTRADICTS THE KB.** <br><br>`01-Architecture/dependency-rules.md:80` — *"IUnitOfWork is the ONLY gateway to data"* — *"All data access MUST go through IUnitOfWork"* — *"UnitOfWork is the exclusive Gateway"* <br><br>`01-Architecture/clean-architecture.md:80-92` — *"Direct injection of IRepositoryBase in services is FORBIDDEN"* <br><br>But the action plan proposes injecting `IUnitOfWorkArchiving` / `IUnitOfWorkTransactionSystem` instead of the full `IUnitOfWork`. This is actually **fine architecturally** — it's Interface Segregation applied to UoW. It doesn't violate the "one gateway" rule. However, the KB explicitly says: *"Services inject `IUnitOfWork`"* (singular). The KB would need to be updated to reflect that sub-interfaces are allowed. | ⚠️ Not a contradiction if KB is updated. Change KB from "one IUnitOfWork" to "domain-specific IUnitOfWork interfaces are allowed". |
| **5.2** Segregate `IRepositoryBase` into `IReadRepository` / `IWriteRepository` | `01-Architecture/dependency-rules.md:254-258` — Repository interface is owned by Application. `application-skill.md:96-140` documents the current monolithic interface. Splitting into read/write would require updating `IRepositoryBase` definition, `RepositoryBase` implementation, and all KB docs referencing it. | ✅ Aligned. High impact — must update `application-skill.md`, `persistence-skill.md`, `vault-structure.md` |

---

### Sprint 6 — 🧹 Standardize Code Style

| Action | KB Check | Verdict |
|--------|----------|---------|
| **6.1** Convert `RoleService` to primary constructors | `04-Agents/01-Backend-Agent/agent-skill.md:83` — *"Primary constructors for DI"* <br>`01-Architecture/clean-architecture.md:83` — primary constructor pattern used throughout | ✅ Aligned |
| **6.2** Add `.editorconfig` + run `dotnet format` | No KB rule about `.editorconfig` exists. **KB Gap:** Should document the formatting toolchain. | 📝 Add `.editorconfig` and `dotnet format` to `02-Code-Conventions/` |

---

### Sprint 7 — 🛡️ Add FluentValidation

| Action | KB Check | Verdict |
|--------|----------|---------|
| **7.1-7.3** Install FluentValidation, create validators, register in DI | `01-Architecture/dependency-rules.md:49-56` — Application layer has "Exception: Microsoft.AspNetCore.Http (for IFormFile)". FluentValidation in Application is fine. <br>`04-Agents/01-Backend-Agent/agent-skill.md:12` — *"Application: Services, DTOs, interfaces, validators"* — already mentions validators in the Application description. | ✅ Aligned. Must update: `application-skill.md` to add `Validators/` folder, `dependency-rules.md` to allow FluentValidation package in Application, `third-party-libs.md` |

---

## 3. KB Gaps Found

These files are **listed in `vault-structure.md` but don't exist on disk:**

| KB Reference | Status | Impact on Action Plan |
|-------------|--------|----------------------|
| `05-Shared-Knowledge/error-handling.md` | ❌ Missing | Low — Sprint 1.3 fix is still safe, but the KB should document the anti-pattern |
| `05-Shared-Knowledge/pagination.md` | ❌ Missing | Low — Sprint 2.2 references it implicitly |
| `05-Shared-Knowledge/query-building.md` | ❌ Missing | **🔴 High** — Sprint 2.2's fix for pushing filters to EF Core needs documented patterns |
| `05-Shared-Knowledge/soft-delete.md` | ❌ Missing | Low — not referenced by current sprints |
| `05-Shared-Knowledge/authentication.md` | ❌ Missing | Low — not referenced |
| `06-Roadmap/roadmap.md` | ❌ Missing | Low — would be useful for planning sprint order |
| `08-Templates/*` (4 files) | ❌ Empty directory | Low — not referenced by current sprints |
| `09-Decision-Log/decisions.md` | ❌ Missing | Low — would document why IUnitOfWork is the exclusive gateway |
| `02-Code-Conventions/*` (5 files) | ❌ Empty directory | **🟡 Medium** — Sprint 6 style conventions need a home |
| `04-Agents/00-Self-Improvement-Skill/self-improvement.md` | ❌ Missing | **🟡 Medium** — Sprint 4+5 change interface definitions; KB must be updated |

---

## 4. Required KB Updates Per Sprint

### Before Sprint 1

| File | Update |
|------|--------|
| `05-Shared-Knowledge/error-handling.md` | **Create** — document `Result<T>` pattern, anti-patterns (raw exceptions, sync-over-async), examples from Sprint 1 fixes |

### Before Sprint 2

| File | Update |
|------|--------|
| `05-Shared-Knowledge/query-building.md` | **Create** — document dynamic filtering with `ExpressionBuilderLib`, `ExpressionCombiner.AndAll`/`OrAll`, how to push filters to EF Core instead of in-memory |
| `03-Projects/02-ModernPaySystem-Application/application-skill.md:99-140` | Add `GetAllByIdsAsync` to the `IRepositoryBase` interface documentation |
| `03-Projects/05-ModernPaySystem-Infrastructure-Persistence/persistence-skill.md` | Add `GetAllByIdsAsync` implementation pattern to repository docs |

### Before Sprint 3

| File | Update |
|------|--------|
| `03-Projects/04-ModernPaySystem-Infrastructure/infrastructure-skill.md:27-51` | Add `Specifications/` folder to the Infrastructure folder structure |
| `vault-structure.md` | Add `Specifications/` under Infrastructure folder tree |

### Before Sprint 4

| File | Update |
|------|--------|
| `03-Projects/02-ModernPaySystem-Application/application-skill.md:26-68` | Add new interface files: `IArchiveFileService`, `IArchiveTemplateService`, `IRequestRelationService` |
| `03-Projects/04-ModernPaySystem-Infrastructure/infrastructure-skill.md` | Add new service implementations and DI registrations |
| `vault-structure.md` | Update file lists if new files were added |

### Before Sprint 5

| File | Update |
|------|--------|
| `01-Architecture/clean-architecture.md:78-92` | **Critical:** Update "UnitOfWork as Mandatory Gateway" to allow domain-specific sub-interfaces (e.g., `IUnitOfWorkArchiving`, `IUnitOfWorkTransactionSystem`) instead of single `IUnitOfWork` |
| `01-Architecture/dependency-rules.md:319-344` | Update "UnitOfWork Registration Rule" — add guidance for sub-interfaces |
| `03-Projects/02-ModernPaySystem-Application/application-skill.md:146-161` | Update `IUnitOfWork` documentation to reflect segregation |
| `03-Projects/05-ModernPaySystem-Infrastructure-Persistence/persistence-skill.md:222-292` | Update UnitOfWork documentation for both monolithic and sub-interface patterns |

### Before Sprint 6

| File | Update |
|------|--------|
| `02-Code-Conventions/csharp-conventions.md` | **Create** — document `.editorconfig`, `dotnet format` command, code style rules |

### Before Sprint 7

| File | Update |
|------|--------|
| `03-Projects/02-ModernPaySystem-Application/application-skill.md:65` | Add `Validators/` folder to Application structure |
| `01-Architecture/dependency-rules.md:49-56` | Add `FluentValidation` to allowed Application packages |
| `07-Dependencies/third-party-libs.md` | Add FluentValidation entry |

---

## 5. Action Plan Corrections

### ❌ Contradiction in Sprint 5 — IUnitOfWork split

**The Plan says:** Inject `IUnitOfWorkArchiving` / `IUnitOfWorkTransactionSystem` in services.

**The KB says:** `01-Architecture/clean-architecture.md:80` — *"All data access MUST go through `IUnitOfWork`"* — listing it as singular.

**Resolution:** The plan is **architecturally correct** (ISP principle) but contradicts the KB word-for-word. Before implementing Sprint 5, the KB must be updated to allow sub-interfaces. Suggested wording:

> *"All data access MUST go through `IUnitOfWork` or a domain-specific sub-interface (`IUnitOfWorkArchiving`, `IUnitOfWorkTransactionSystem`). Sub-interfaces inherit the transaction contract and expose only the repositories relevant to their domain."*

### ⚠️ Sprint 5 — IRepositoryBase segregation is high-risk

The Plan proposes splitting `IRepositoryBase` into `IReadRepository` / `IWriteRepository`. This would **break every existing service** and require updating ~26 service files simultaneously. A safer approach:

**Recommendation:** Defer Sprint 5.2 to a later phase. Implement Sprint 5.1 (IUnitOfWork split) first. The `IRepositoryBase` segregation should be a separate epic with its own KB update.

### 🔴 SPRINT 2 IS BLOCKED

Sprint 2.2 requires `05-Shared-Knowledge/query-building.md` which doesn't exist. **Must create this file first.** Suggested content:

```markdown
# Query Building — ModernPaySystem

## Dynamic Filtering with ExpressionBuilderLib

Services build `List<Expression<Func<T, bool>>>` and pass to RepositoryBase...

## Anti-Pattern: In-Memory Filtering

❌ BAD — loads all rows, filters in memory:
    var all = await repo.GetAllAsync();
    var filtered = all.Where(x => x.Name.Contains(term));

✅ GOOD — pushes filter to SQL:
    var result = await repo.GetAllAsync(filter: x => x.Name.Contains(term));
```

---

## 6. Action Plan Conformity Scorecard

| Principle | Plan Score | Notes |
|-----------|-----------|-------|
| UnitOfWork as exclusive gateway | ⚠️ 9/10 | Sprint 5.1 changes this; KB must be updated first |
| Application = contracts only | ✅ 10/10 | All new interfaces planned |
| DTOs in Domain | ✅ 10/10 | No DTO changes in plan |
| PostgreSQL + Npgsql | ✅ 10/10 | No DB changes planned |
| Primary constructors | ✅ 10/10 | Sprint 6.1 aligns |
| Async all the way | ✅ 10/10 | Sprint 1.2 fixes violations |
| Result<T> pattern | ✅ 10/10 | Sprint 1.3 fixes violations |
| File-scoped namespaces | ✅ 10/10 | No violations |
| Self-improvement (KB updates) | ❌ Not addressed | **Missing from plan!** Every sprint should include a "KB Update" step |

---

## 7. Critical Missing Piece: KB Self-Improvement

The `04-Agents/01-Backend-Agent/agent-skill.md` ends with:

> *"Self-Improvement: KB must stay in sync"*

But the **action plan never mentions updating the KB** after any sprint. This must be added to every sprint.

### Add to every sprint's checklist:

```markdown
- [ ] Update KB files: 
  - [ ] `03-Projects/04-ModernPaySystem-Infrastructure/infrastructure-skill.md` (new services)
  - [ ] `03-Projects/02-ModernPaySystem-Application/application-skill.md` (new interfaces)
  - [ ] `01-Architecture/dependency-rules.md` (if DI registration changes)
  - [ ] `vault-structure.md` (if new files/folders created)
  - [ ] `09-Decision-Log/decisions.md` (document architectural decisions)
```

---

## 8. Recommended Sprint Order Adjustment

| Order | Sprint | Reason |
|-------|--------|--------|
| **1** | Sprint 1 — Bug fixes | Safety: fix known bugs first |
| **2** | **Create missing KB files** | Unblock Sprint 2; satisfy self-improvement rule |
| **3** | Sprint 2 — Performance | Now unblocked |
| **4** | Sprint 3 — Query specs | Low risk, high value |
| **5** | Sprint 6 + 7 — Style + Validation | Independent, safe |
| **6** | Sprint 4 — Split services | Requires KB updates for new interfaces |
| **7** | Sprint 5.1 — IUnitOfWork split | Requires KB rewrite of gateway pattern |
| **8** | Sprint 5.2 — IRepositoryBase split | **Deferred** — high risk, break 26 files |

---

## 9. Pre-Flight Checklist (Before Any Sprint)

```markdown
- [ ] KB files referenced in `vault-structure.md` actually exist on disk
- [ ] Missing KB files have been created (especially `query-building.md`)
- [ ] 02-Code-Conventions/ directory is populated
- [ ] 09-Decision-Log/decisions.md documents the refactoring intent
- [ ] Each sprint has a `KB Update` sub-task
- [ ] Sprint 5 has explicit KB update for `clean-architecture.md:78-92`
```
