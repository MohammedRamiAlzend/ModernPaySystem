using Microsoft.Extensions.Logging;
using ModernPaySystem.Domain.Commons;
using OcrReader;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of OCR service for text extraction from images and PDFs.
/// </summary>
public class OcrService : IOcrService
{
    private readonly IOcrGenerator _ocrGenerator;
    private readonly ILogger<OcrService> _logger;

    public OcrService(IOcrGenerator ocrGenerator, ILogger<OcrService> logger)
    {
        _ocrGenerator = ocrGenerator;
        _logger = logger;
    }

    public async Task<Result<string>> ExtractTextFromImageAsync(string imagePath, string language = "eng")
    {
        try
        {
            _logger.LogInformation("Extracting text from image: {ImagePath} using language: {Language}", imagePath, language);
            
            if (string.IsNullOrWhiteSpace(imagePath))
                return ApplicationErrors.InvalidInput;

            if (!File.Exists(imagePath))
                return ApplicationErrors.OperationFailed;

            var extractedText = await _ocrGenerator.ExtractTextFromImageAsync(imagePath, language);
            
            _logger.LogInformation("Successfully extracted text from image: {ImagePath}", imagePath);
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from image: {ImagePath}", imagePath);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<string>> ExtractTextFromPdfAsync(string pdfPath, string language = "eng")
    {
        try
        {
            _logger.LogInformation("Extracting text from PDF: {PdfPath} using language: {Language}", pdfPath, language);
            
            if (string.IsNullOrWhiteSpace(pdfPath))
                return ApplicationErrors.InvalidInput;

            if (!File.Exists(pdfPath))
                return ApplicationErrors.OperationFailed;

            var extractedText = await _ocrGenerator.ExtractTextFromPdfAsync(pdfPath, language);
            
            _logger.LogInformation("Successfully extracted text from PDF: {PdfPath}", pdfPath);
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF: {PdfPath}", pdfPath);
            return ApplicationErrors.InternalServerError;
        }
    }
}