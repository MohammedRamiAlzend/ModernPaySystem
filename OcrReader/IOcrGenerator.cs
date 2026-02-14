namespace OcrReader;

public interface IOcrGenerator
{
    Task<string> ExtractTextFromImageAsync(string path, string language);
    Task<string> ExtractTextFromPdfAsync(string pdfPath, string language);
}
