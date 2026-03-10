namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for OCR service for text extraction from images and PDFs.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Extract text from an image file.
    /// </summary>
    Task<Result<string>> ExtractTextFromImageAsync(string imagePath, string language = "eng");

    /// <summary>
    /// Extract text from a PDF file.
    /// </summary>
    Task<Result<string>> ExtractTextFromPdfAsync(string pdfPath, string language = "eng");
}
