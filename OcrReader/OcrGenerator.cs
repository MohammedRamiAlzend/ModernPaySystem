using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace OcrReader;

public class OcrGenerator : IOcrGenerator
{
    private readonly string _scriptPath;
    private readonly string _tesseractDir;

    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg", ".tiff", ".pdf" };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public OcrGenerator()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _scriptPath = Path.Combine(baseDir, "ocr.py");
        _tesseractDir = Path.Combine(baseDir, "Tesseract-OCR");

        if (!File.Exists(_scriptPath))
        {
            var assemblyDir = Path.GetDirectoryName(typeof(OcrGenerator).Assembly.Location);
            _scriptPath = Path.Combine(assemblyDir!, "ocr.py");
            _tesseractDir = Path.Combine(assemblyDir!, "Tesseract-OCR");
        }
    }

    public async Task<string> ExtractTextFromImageAsync(string path, string language)
    {
        return await RunPythonOcrAsync(path, language, "text");
    }

    public async Task<string> ExtractTextFromPdfAsync(string pdfPath, string language)
    {
        return await RunPythonOcrAsync(pdfPath, language, "text");
    }

    public async Task<OcrResult> ExtractStructuredAsync(string filePath, string language = "ara+eng")
    {
        var json = await RunPythonOcrAsync(filePath, language, "json");
        return JsonSerializer.Deserialize<OcrResult>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse OCR result JSON.");
    }

    private async Task<string> RunPythonOcrAsync(string filePath, string language, string outputFormat)
    {
        ValidateFile(filePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{_scriptPath}\" \"{filePath}\" --lang {language} --output {outputFormat}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

        if (Directory.Exists(_tesseractDir))
        {
            var tessdataDir = Path.Combine(_tesseractDir, "tessdata");
            startInfo.EnvironmentVariables["PATH"] = _tesseractDir + ";" + Environment.GetEnvironmentVariable("PATH");
            startInfo.EnvironmentVariables["TESSDATA_PREFIX"] = tessdataDir;
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        await Task.WhenAll(stdoutTask, stderrTask);
        await process.WaitForExitAsync();

        var stdout = stdoutTask.Result.Trim();
        var stderr = stderrTask.Result.Trim();

        if (process.ExitCode != 0)
        {
            var message = stderr.Length > 0 ? stderr : $"OCR process exited with code {process.ExitCode}";
            throw new InvalidOperationException(message);
        }

        return stdout;
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
