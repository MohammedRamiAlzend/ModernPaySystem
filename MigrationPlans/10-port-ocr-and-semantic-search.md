---
tags: [migration, ocr, semantic-search, external]
module: Boot
status: draft
priority: medium
depends-on: []
---

# 10 — Port OCR and Semantic Search Integration

## Problem

Two library projects exist under `src/` but are **not wired into any module**:

- `src/OcrReader/` — contains `OcrGenerator` class
- `src/SemanticSearchLib/` — contains `FileParserService`, `OllamaEmbeddingProvider`, `TextChunkerService`

The old system's `OcrService` and `SemanticSearchAppService` + `QdrantVectorStore` were not ported. The new libraries have **different contracts** and need module-level integration.

## OCR

### Current State

| Old | New |
|---|---|
| `IOcrService` with `ExtractTextFromImageAsync`, `ExtractTextFromPdfAsync` | `OcrGenerator` in `src/OcrReader/` — different method signatures |
| `OcrController` at `POST api/ocr/extract-text-from-image` | No controller in new structure |
| Tesseract OCR engine | Tesseract OCR engine (same) |

### Action Plan

1. Create `IArchivingOcrService` interface in `Archive.Application/Interfaces/`
2. Create `ArchivingOcrService` implementation wrapping `OcrGenerator`
3. Create `OcrController` in `Archive.Api/Controllers/` or as a standalone API project
4. Register OCR DI in appropriate module

OR: Wrap `OcrGenerator` in an adapter that implements a generic `IOcrService` in SharedKernel.

## Semantic Search

### Current State

| Old | New |
|---|---|
| `ISemanticSearchService` — search, index, delete documents | `SemanticSearchLib/` — building blocks only |
| `QdrantVectorStore` — vector DB integration | No Qdrant client registered |
| `DocumentChunk`, `DocumentExpressions` entities | Already ported to `Archive.Domain/Entities/` ✅ |

### Action Plan

1. Create `ISemanticSearchService` in `Archive.Application/Interfaces/`
2. Create `SemanticSearchService` implementation using `SemanticSearchLib` building blocks
3. Port `QdrantVectorStore` + `IQdrantVectorStore` to `Archive.Infrastructure/Services/Qdrant/`
4. Bring in `Qdrant.Client` NuGet package
5. Create/port `DocumentIndexingController` in `Archive.Api/Controllers/`
6. Create/port `SemanticSearchController` in `Archive.Api/Controllers/`
7. Register DI in `ArchiveModuleRegistration.cs`

### QdrantOptions

Port `Options/QdrantOptions.cs` to `Archive.Infrastructure/Options/QdrantOptions.cs`:

```csharp
public class QdrantOptions
{
    public const string SectionName = "Qdrant";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6333;
    public string CollectionName { get; set; } = "documents";
    public int VectorSize { get; set; } = 768;
}
```

## Verification

```bash
# OCR
curl -X POST -F "imageFile=@test.jpg" http://localhost:5000/api/ocr/extract-text-from-image

# Semantic search
curl -X POST -H "Content-Type: application/json" \
  -d '{"query": "test", "page": 1, "pageSize": 10}' \
  http://localhost:5000/api/semantic-search/search
```

## References

- Old: `ModernPaySystem.Infrastructure/Services/OcrService.cs`
- Old: `ModernPaySystem.Infrastructure/Services/SemanticSearchAppService.cs`
- Old: `ModernPaySystem.Infrastructure/Services/Qdrant/`
- Old: `ModernPaySystem.Infrastructure/Options/QdrantOptions.cs`
- Old: `ModernPaySystem/Controllers/OcrController.cs`
- Old: `ModernPaySystem/Controllers/ArchivingControllers/SemanticSearchController.cs`
- Old: `ModernPaySystem/Controllers/ArchivingControllers/DocumentIndexingController.cs`
- New: `src/OcrReader/OcrGenerator.cs`
- New: `src/SemanticSearchLib/`
