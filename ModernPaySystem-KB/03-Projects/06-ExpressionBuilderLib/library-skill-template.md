# Library Skill Template — Utility Projects

## Purpose

Utility libraries provide **generic, reusable functionality** consumed by any layer. They have **zero knowledge** of the application's business domain — no entities, no DTOs, no DbContext. They are pure algorithms and integrations packaged as standalone .NET class libraries.

---

## Library Manifest

| Library | Purpose |
|---------|---------|
| **ExpressionBuilderLib** | Dynamic expression tree building for filtering/sorting |
| **FileManager** | File read/write, directory operations, path utilities |
| **NumberSpelling** | Convert numbers to words (currency amounts, invoices) |
| **OcrReader** | Optical character recognition from images/PDFs |

---

## Responsibilities

| Responsibility | Details |
|---------------|---------|
| **Encapsulated Logic** | Single well-defined concern (expression building, file I/O, spelling, OCR) |
| **No Domain Knowledge** | Operates on primitives (`string`, `Stream`, `byte[]`), not domain objects |
| **No Side Effects** | Pure functions where possible; I/O isolated to dedicated methods |
| **Testable in Isolation** | No DI required; construct with primitives |
| **Framework-Agnostic** | Prefer `System.*` only; avoid ASP.NET/EF Core coupling |

---

## Folder Structure

```
{ProjectName}/
├── {ProjectName}.csproj
├── Models/
│   ├── {Domain}.cs               // Library-specific models
│   └── Result.cs                  // Optional result type
├── Services/
│   ├── {CoreService}.cs           // Primary functionality
│   └── {CoreService}.Options.cs   // Configuration class
├── Extensions/
│   ├── StringExtensions.cs
│   ├── StreamExtensions.cs
│   └── ServiceCollectionExtensions.cs  // DI registration
├── Abstractions/
│   ├── I{Service}.cs              // Optional interface for DI
│   └── I{Provider}.cs             // Pluggable provider interface
├── Providers/
│   └── {DefaultProvider}.cs       // Default implementation
├── Exceptions/
│   └── {ProjectName}Exception.cs
└── Tests/
    ├── {CoreService}Tests.cs
    └── Extensions/
```

---

## Public API Design

### Principles

1. **Minimal surface area** — expose only what consumers need
2. **Primitive parameters** — accept `string`, `Stream`, `byte[]`, not domain types
3. **Strong types** — use enums and options classes, not magic strings
4. **Consistent return types** — `Result<T>`, `T?`, or `(bool Success, T? Data, string? Error)`

### ExpressionBuilderLib Example

```csharp
// ExpressionBuilderLib/Services/ExpressionBuilder.cs
public class ExpressionBuilder
{
    public Expression<Func<T, bool>> BuildPredicate<T>(
        string propertyName, Operator op, object value)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, propertyName);
        var constant = Expression.Constant(value);
        var body = op switch
        {
            Operator.Equals => Expression.Equal(property, constant),
            Operator.Contains => Expression.Call(property, nameof(string.Contains),
                Type.EmptyTypes, constant),
            Operator.GreaterThan => Expression.GreaterThan(property, constant),
            _ => throw new NotSupportedException($"Operator {op} not supported")
        };
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

// ExpressionBuilderLib/Models/Operator.cs
public enum Operator { Equals, NotEquals, Contains, GreaterThan, LessThan, StartsWith }
```

### FileManager Example

```csharp
// FileManager/Services/FileManagerService.cs
public class FileManagerService
{
    private readonly string _basePath;

    public FileManagerService(string basePath) => _basePath = GuardPath(basePath);

    public async Task<Result<string>> SaveAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, SanitizeFileName(fileName));
        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(fullPath);
        await content.CopyToAsync(stream, ct);
        return Result<string>.Success(fullPath);
    }

    public async Task<Result<Stream>> ReadAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            return Result<Stream>.Failure("File not found");
        var stream = File.OpenRead(filePath);
        return Result<Stream>.Success(stream);
    }

    public Result Delete(string filePath)
    {
        if (!File.Exists(filePath))
            return Result.Failure("File not found");
        File.Delete(filePath);
        return Result.Success();
    }

    private static string SanitizeFileName(string fileName) =>
        string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

    private static string GuardPath(string path) =>
        Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
}
```

### NumberSpelling Example

```csharp
// NumberSpelling/Services/NumberSpeller.cs
public class NumberSpeller
{
    private readonly CultureInfo _culture;
    private static readonly Dictionary<string, INumberSpellingProvider> Providers = new()
    {
        ["en-US"] = new EnglishNumberSpellingProvider(),
        ["ar-SA"] = new ArabicNumberSpellingProvider(),
    };

    public NumberSpeller(string cultureCode = "en-US")
    {
        _culture = CultureInfo.GetCultureInfo(cultureCode);
        if (!Providers.ContainsKey(_culture.Name))
            throw new NotSupportedException($"Culture {cultureCode} not supported");
    }

    public string Spell(decimal number) => Spell(number, CurrencyPosition.After);
    public string Spell(decimal number, CurrencyPosition position)
        => Providers[_culture.Name].Spell(number, _culture, position);
}

// NumberSpelling/Abstractions/INumberSpellingProvider.cs
public interface INumberSpellingProvider
{
    string Spell(decimal number, CultureInfo culture, CurrencyPosition position);
}

// NumberSpelling/Providers/EnglishNumberSpellingProvider.cs
internal class EnglishNumberSpellingProvider : INumberSpellingProvider
{
    private static readonly string[] Ones = ["", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"];
    private static readonly string[] Teens = ["Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"];
    private static readonly string[] Tens = ["", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"];

    public string Spell(decimal number, CultureInfo culture, CurrencyPosition position)
    {
        var whole = (int)Math.Floor(number);
        var cents = (int)((number - whole) * 100);
        var wholeWords = ConvertWhole(whole);
        var centsWords = cents > 0 ? $"{ConvertWhole(cents)} Cent{(cents > 1 ? "s" : "")}" : "";
        return position switch
        {
            CurrencyPosition.After => $"{wholeWords} Riyal{(whole > 1 ? "s" : "")} {centsWords}".Trim(),
            CurrencyPosition.Before => $"{wholeWords} Dollars{(whole > 1 ? "s" : "")} {centsWords}".Trim(),
            _ => $"{wholeWords} and {centsWords}".Trim()
        };
    }
}
```

### OcrReader Example

```csharp
// OcrReader/Services/OcrReaderService.cs
public class OcrReaderService
{
    private readonly ITesseractEngine _engine;

    public OcrReaderService(string tessDataPath, string language = "eng")
    {
        _engine = new TesseractEngine(tessDataPath, language, EngineMode.Default);
    }

    public async Task<Result<string>> ReadTextAsync(Stream imageStream, CancellationToken ct = default)
    {
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream, ct);
        using var pix = Pix.LoadFromMemory(memoryStream.ToArray());
        using var page = _engine.Process(pix);
        var text = page.GetText();
        return string.IsNullOrWhiteSpace(text)
            ? Result<string>.Failure("No text recognized")
            : Result<string>.Success(text.Trim());
    }

    public async Task<Result<string>> ReadTextFromFileAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            return Result<string>.Failure("File not found");
        await using var stream = File.OpenRead(filePath);
        return await ReadTextAsync(stream, ct);
    }

    public void Dispose() => _engine.Dispose();
}
```

---

## Extension Points

| Library | Extension | How |
|---------|-----------|-----|
| **ExpressionBuilderLib** | Custom operators | Implement `IExpressionOperator` |
| **FileManager** | Cloud storage providers | Implement `IFileStorageProvider` |
| **NumberSpelling** | New languages | Implement `INumberSpellingProvider` |
| **OcrReader** | Different OCR engines | Implement `IOcrEngine` |

```csharp
// Abstractions/IExpressionOperator.cs
public interface IExpressionOperator
{
    Expression Build(Expression property, Expression constant);
}

// Registering custom provider
public static class NumberSpellingServiceCollectionExtensions
{
    public static IServiceCollection AddNumberSpelling(this IServiceCollection services, string cultureCode = "en-US")
    {
        services.AddSingleton(new NumberSpeller(cultureCode));
        return services;
    }
}
```

---

## Performance Considerations

| Library | Concern | Mitigation |
|---------|---------|------------|
| **ExpressionBuilderLib** | Expression compilation overhead | Cache compiled expressions in `ConcurrentDictionary` |
| **FileManager** | Large file memory pressure | Use `Stream` API, never `byte[]` for large files |
| **FileManager** | Path traversal | Sanitize all filename inputs |
| **NumberSpelling** | Repeated calls | Cache provider instances (singleton) |
| **OcrReader** | Memory usage with large images | Resize before processing; dispose engines |
| **All** | Thread safety | Ensure stateless or document thread-safety |

### Expression caching example

```csharp
public class ExpressionBuilder
{
    private static readonly ConcurrentDictionary<string, object> Cache = new();

    public Expression<Func<T, bool>> BuildPredicate<T>(string propertyName, Operator op, object value)
    {
        var key = $"{typeof(T).FullName}.{propertyName}.{op}.{value}";
        return (Expression<Func<T, bool>>)Cache.GetOrAdd(key, _ =>
            BuildPredicateInternal<T>(propertyName, op, value));
    }
}
```

---

## Testing Strategy

### What to test

| Library | Test Focus |
|---------|-----------|
| **ExpressionBuilderLib** | Correct expression tree generation for each operator; edge cases (null, nested properties) |
| **FileManager** | Save/read/delete operations; path traversal prevention; concurrent access |
| **NumberSpelling** | All number ranges (0, 1, teens, hundreds, thousands, decimals, large numbers); culture-specific output |
| **OcrReader** | Image with text; blank image; corrupted image; different languages if supported |

### Test examples

```csharp
// ExpressionBuilderLib Tests
[Fact]
public void BuildPredicate_StringContains_GeneratesCorrectExpression()
{
    var builder = new ExpressionBuilder();
    var predicate = builder.BuildPredicate<User>(nameof(User.Name), Operator.Contains, "John");
    var compiled = predicate.Compile();

    Assert.True(compiled(new User { Name = "Johnny" }));
    Assert.False(compiled(new User { Name = "Jane" }));
}

// FileManager Tests
[Fact]
public async Task SaveAsync_ValidStream_ReturnsFilePath()
{
    var basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    var fm = new FileManagerService(basePath);
    var content = new MemoryStream("hello"u8.ToArray());

    var result = await fm.SaveAsync("test.txt", content);

    Assert.True(result.IsSuccess);
    Assert.True(File.Exists(result.Data));
    Directory.Delete(basePath, true);
}

// NumberSpelling Tests
[Theory]
[InlineData(0, "Zero Dollars")]
[InlineData(1, "One Dollar")]
[InlineData(123.45, "One Hundred Twenty-Three Dollars and Forty-Five Cents")]
[InlineData(1000, "One Thousand Dollars")]
public void Spell_VariousAmounts_ReturnsCorrectWords(decimal amount, string expected)
{
    var speller = new NumberSpeller();
    Assert.Equal(expected, speller.Spell(amount, CurrencyPosition.Before));
}

// OcrReader Tests
[Fact]
public async Task ReadTextAsync_ImageWithText_ReturnsText()
{
    await using var image = File.OpenRead("TestData/sample-text.png");
    var reader = new OcrReaderService("./tessdata");
    var result = await reader.ReadTextAsync(image);
    Assert.True(result.IsSuccess);
    Assert.Contains("Hello", result.Data, StringComparison.OrdinalIgnoreCase);
    reader.Dispose();
}
```

---

## Security Considerations

| Library | Risk | Mitigation |
|---------|------|------------|
| **FileManager** | Path traversal | Reject paths with `..`; use `Path.GetFullPath` + verify it starts with base path |
| **FileManager** | Large file upload | Enforce maximum file size at entry point (not in library) |
| **FileManager** | Overwriting files | Expose `overwrite: bool` parameter; never auto-overwrite |
| **OcrReader** | Malformed images | Wrap Tesseract in try/catch; validate first N bytes |
| **All** | Unvalidated input | Guard against null/empty strings; validate at public API boundary |

```csharp
// FileManager — path traversal prevention
public async Task<Result<string>> SaveAsync(string fileName, Stream content, bool overwrite = false, CancellationToken ct = default)
{
    var sanitized = SanitizeFileName(fileName);
    var fullPath = Path.GetFullPath(Path.Combine(_basePath, sanitized));
    if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
        return Result<string>.Failure("Invalid path");
    if (File.Exists(fullPath) && !overwrite)
        return Result<string>.Failure("File already exists");
    // ...
}
```

---

## Refactoring Opportunities

### 1. Extract Configuration to Options Class

```csharp
// ❌ Before — hardcoded
public class OcrReaderService
{
    public OcrReaderService()
    {
        _engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
    }
}

// ✅ After — configurable
public class OcrReaderOptions
{
    public const string SectionName = "OcrReader";
    public string TessDataPath { get; init; } = "./tessdata";
    public string Language { get; init; } = "eng";
}

public class OcrReaderService
{
    public OcrReaderService(IOptions<OcrReaderOptions> options)
    {
        var opts = options.Value;
        _engine = new TesseractEngine(opts.TessDataPath, opts.Language, EngineMode.Default);
    }
}
```

### 2. Provider Pattern for Multiple Implementations

```csharp
// ❌ Before — single implementation
public class NumberSpeller
{
    public string Spell(decimal number) => EnglishSpell(number);
}

// ✅ After — provider registration
public class NumberSpeller
{
    private readonly INumberSpellingProvider _provider;
    public NumberSpeller(INumberSpellingProvider provider) => _provider = provider;
    public string Spell(decimal number) => _provider.Spell(number);
}
```

### 3. Use Result Pattern for Error Handling

```csharp
// ❌ Before — exceptions for expected failures
public async Task<Stream> ReadAsync(string path)
{
    if (!File.Exists(path)) throw new FileNotFoundException();
    return File.OpenRead(path);
}

// ✅ After — Result<T>
public async Task<Result<Stream>> ReadAsync(string path)
{
    if (!File.Exists(path)) return Result<Stream>.Failure("File not found");
    return Result<Stream>.Success(File.OpenRead(path));
}
```

---

## AI Generation Instructions

### When creating a new library

```markdown
1. Place in solution root: `{LibraryName}/{LibraryName}.csproj`
2. Use `<TargetFramework>net10.0</TargetFramework>` (matching solution)
3. Zero external package dependencies unless required
4. Primary class should be stateless or thread-safe
5. Accept configuration via constructor (not static config)
6. Return Result<T> for expected failures
7. Throw only for programming errors (null args, invalid state)
8. Include XML doc comments on all public members
9. Register via extension method: `services.Add{LibraryName}(...)`
10. Write tests covering: happy path, edge cases, error cases
```

### Library checklist

```markdown
- [ ] Single responsibility — one job, does it well
- [ ] No domain references (no Application, Domain, Persistence usings)
- [ ] No ASP.NET/Core references
- [ ] Public API uses primitives or library-specific types only
- [ ] No hardcoded paths, API keys, or configuration
- [ ] Thread-safe or documented as not thread-safe
- [ ] All public methods have XML doc comments
- [ ] DI registration extension method provided
- [ ] Unit tests cover >90% of code paths
- [ ] Provider/abstraction pattern for extension points identified
```
