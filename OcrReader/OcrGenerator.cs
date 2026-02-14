using System.Diagnostics;
using System.Text;
using Tesseract;

namespace OcrReader;

public class OcrGenerator : IOcrGenerator
{
    public async Task<string> ExtractTextFromImageAsync(string path, string language)
    {
        try
        {
            string appDomainPath = AppDomain.CurrentDomain.BaseDirectory;
            string tessdataPath = Path.Combine(appDomainPath, @"Tesseract-OCR\tessdata");
            using var engine = new TesseractEngine(tessdataPath, language, EngineMode.Default);
            using var img = Pix.LoadFromFile(path);
            using var page = engine.Process(img);
            return page.GetText();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string> ExtractTextFromPdfAsync(string pdfPath, string language)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pdftoppm",
                Arguments = $"-jpeg \"{pdfPath}\" \"{Path.Combine(tempDir, "page")}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit();
        var result = new StringBuilder();
        string appDomainPath = AppDomain.CurrentDomain.BaseDirectory;
        string tessdataPath = Path.Combine(appDomainPath, "Tesseract-OCR/tessdata");
        foreach (string imagePath in Directory.GetFiles(tempDir, "page*.jpg"))
        {
            using var engine = new TesseractEngine(tessdataPath, language, EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            result.AppendLine(page.GetText());
        }

        Directory.Delete(tempDir, true);
        return result.ToString();
    }

}
