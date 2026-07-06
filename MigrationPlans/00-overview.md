---
tags: [migration, overview]
module: all
status: draft
---

# 00 — Migration Overview

**Goal**: Complete the monolith-to-module migration by closing all gaps identified in the [[gap-analysis]].

## Architecture Reminder

```
src/
├── ModernPaySystem.SharedKernel.Domain/       # Shared entities, DTOs, Commons
├── ModernPaySystem.SharedKernel.Application/   # Shared repos, interfaces
├── ModernPaySystem.SharedKernel.Infrastructure/ # Shared services, persistence
├── Modules/
│   ├── IdentitySystem/   # Auth, Users, Roles, Departments
│   ├── TransactionSystem/ # Requests, Responses, Templates, Reports
│   └── ArchiveSystem/    # Archive Records, Config, Workflows, Reports
├── ModernPaySystem.Boot/  # Entry point
├── OcrReader/             # OCR library (not wired)
└── SemanticSearchLib/     # Semantic search library (not wired)
```

## Migration Plan Sequence

| # | Plan | Priority | Impact |
|---|---|---|---|
| 01 | [[01-fix-transaction-services]] | 🔴 Critical | DI crash — 6 missing impls |
| 02 | [[02-fix-archive-services]] | 🔴 Critical | DI crash — 7 missing impls |
| 03 | [[03-fix-di-registrations]] | 🔴 Critical | Missing registrations in all modules |
| 04 | [[04-add-ef-core-migrations]] | 🔴 Critical | App won't start without migrations |
| 05 | [[05-add-seeding-infrastructure]] | 🟡 High | Empty DB on first run |
| 06 | [[06-port-missing-controllers]] | 🟡 High | Missing functionality |
| 07 | [[07-resolve-departmentservice-conflict]] | 🟡 High | Potential DI shadowing |
| 08 | [[08-restore-permission-authorization]] | 🟡 High | Permissions not enforced |
| 09 | [[09-restore-cross-cutting-concerns]] | 🟢 Medium | Logging, limits, health |
| 10 | [[10-port-ocr-and-semantic-search]] | 🟢 Medium | Missing features |
| 11 | [[11-missing-entities-and-validators]] | 🟢 Medium | Missing domain types |

## Status Legend

- 🔴 **Critical** — App will crash or fail to start
- 🟡 **High** — Functionality missing or broken
- 🟢 **Medium** — Quality/cross-cutting improvements
