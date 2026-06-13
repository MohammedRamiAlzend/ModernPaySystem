using System.Diagnostics;
using System.Text;
using Tesseract;

namespace OcrReader;

public class OcrGenerator : IOcrGenerator
{
    private static readonly HashSet<string> SupportedImageFormats = new(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg", ".tiff" };

    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg", ".tiff", ".pdf" };

    private const int MaxPixelDimension = 4000;
    private const int OcrDpi = 300;

    private readonly string _tessdataPath;

    public OcrGenerator()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _tessdataPath = Path.Combine(baseDir, "Tesseract-OCR", "tessdata");

        if (!Directory.Exists(_tessdataPath))
        {
            var assemblyDir = Path.GetDirectoryName(typeof(OcrGenerator).Assembly.Location);
            _tessdataPath = Path.Combine(assemblyDir!, "Tesseract-OCR", "tessdata");
        }
    }

    public Task<string> ExtractTextFromImageAsync(string path, string language)
    {
        ValidateFile(path);
        using var engine = CreateEngine(language);
        using var img = LoadAndPreprocessImage(path);
        using var page = engine.Process(img);
        return Task.FromResult((page.GetText() ?? "").Trim());
    }

    public async Task<string> ExtractTextFromPdfAsync(string pdfPath, string language)
    {
        ValidateFile(pdfPath);
        using var engine = CreateEngine(language);
        var result = new StringBuilder();
        var pageNum = 1;

        await foreach (var img in ConvertPdfToImagesAsync(pdfPath))
        {
            using var page = engine.Process(img);
            var text = (page.GetText() ?? "").Trim();
            result.AppendLine($"[Page {pageNum}]");
            result.AppendLine(text);
            pageNum++;
        }

        return result.ToString().Trim();
    }

    public async Task<OcrResult> ExtractStructuredAsync(string filePath, string language = "ara+eng")
    {
        ValidateFile(filePath);
        var path = new FileInfo(filePath);
        var pages = new List<OcrPage>();
        var pageNum = 1;

        await foreach (var img in LoadImagesAsync(path))
        {
            pages.Add(ExtractPageData(img, language, pageNum));
            pageNum++;
        }

        return new OcrResult(path.FullName, pages);
    }

    private TesseractEngine CreateEngine(string language)
    {
        var lang = string.IsNullOrWhiteSpace(language) ? "ara+eng" : language;
        var engine = new TesseractEngine(_tessdataPath, lang, EngineMode.Default);
        engine.SetVariable("tessedit_pageseg_mode", "3");
        return engine;
    }

    private Pix LoadAndPreprocessImage(string path)
    {
        var img = Pix.LoadFromFile(path);
        if (Math.Max(img.Width, img.Height) > MaxPixelDimension)
        {
            var scale = (float)MaxPixelDimension / Math.Max(img.Width, img.Height);
            var scaled = img.Scale(scale, scale);
            img.Dispose();
            return scaled ?? throw new InvalidOperationException("Failed to scale image");
        }
        return img;
    }

    private OcrPage ExtractPageData(Pix img, string language, int pageNum)
    {
        using var engine = CreateEngine(language);
        using var page = engine.Process(img);

        var fullText = (page.GetText() ?? "").Trim();
        var words = new List<OcrWord>();

        using var iter = page.GetIterator();
        if (iter is not null)
        {
            iter.Begin();
            do
            {
                var word = iter.GetText(PageIteratorLevel.Word)?.Trim();
                if (string.IsNullOrEmpty(word))
                    continue;

                var confidence = iter.GetConfidence(PageIteratorLevel.Word);
                if (confidence < 0)
                    continue;

                if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out var rect))
                {
                    var detectedLang = word.Any(c => c >= 0x0600 && c <= 0x06FF) ? "ara" : "eng";
                    words.Add(new OcrWord(
                        word,
                        [rect.X1, rect.Y1, rect.X2, rect.Y2],
                        Math.Round(confidence, 1),
                        detectedLang
                    ));
                }
            } while (iter.Next(PageIteratorLevel.Word));
        }

        return new OcrPage(pageNum, fullText, words);
    }

    private async IAsyncEnumerable<Pix> LoadImagesAsync(FileInfo file)
    {
        var ext = file.Extension.ToLowerInvariant();
        if (ext == ".pdf")
        {
            await foreach (var img in ConvertPdfToImagesAsync(file.FullName))
                yield return img;
        }
        else
        {
            yield return LoadAndPreprocessImage(file.FullName);
        }
    }

    private async IAsyncEnumerable<Pix> ConvertPdfToImagesAsync(string pdfPath)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pdftoppm",
                    Arguments = $"-jpeg -r {OcrDpi} \"{pdfPath}\" \"{Path.Combine(tempDir, "page")}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();

            foreach (var imagePath in Directory.GetFiles(tempDir, "page*.jpg"))
                yield return LoadAndPreprocessImage(imagePath);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private static void ValidateFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");

        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext) || !SupportedFormats.Contains(ext))
            throw new NotSupportedException(
                $"Unsupported format '{ext}'. Supported: {string.Join(", ", SupportedFormats)}");
    }
}
