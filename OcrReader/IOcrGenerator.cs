namespace OcrReader;

public record OcrWord(string Text, int[] Bbox, double Confidence, string Lang);

public record OcrPage(int PageNum, string Text, List<OcrWord> Words);

public record OcrResult(string File, List<OcrPage> Pages);

public interface IOcrGenerator
{
    Task<string> ExtractTextFromImageAsync(string path, string language);

    Task<string> ExtractTextFromPdfAsync(string pdfPath, string language);

    Task<OcrResult> ExtractStructuredAsync(string filePath, string language = "ara+eng");
}
