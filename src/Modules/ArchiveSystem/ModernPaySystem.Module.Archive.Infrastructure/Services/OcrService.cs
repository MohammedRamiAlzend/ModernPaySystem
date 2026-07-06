using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.SharedKernel.Domain.Commons;
using OcrReader;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class OcrService(
    IOcrGenerator ocrGenerator,
    ILogger<OcrService> logger) : IOcrService
{
    public async Task<Result<string>> ExtractTextFromImageAsync(string imagePath, string language = "eng")
    {
        try
        {
            logger.LogInformation("Extracting text from image: {ImagePath} using language: {Language}", imagePath, language);

            if (string.IsNullOrWhiteSpace(imagePath))
                return ArchiveErrors.InvalidInput;

            if (!System.IO.File.Exists(imagePath))
                return ArchiveErrors.InvalidInput;

            var extractedText = await ocrGenerator.ExtractTextFromImageAsync(imagePath, language);

            logger.LogInformation("Successfully extracted text from image: {ImagePath}", imagePath);
            return extractedText;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting text from image: {ImagePath}", imagePath);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<string>> ExtractTextFromPdfAsync(string pdfPath, string language = "eng")
    {
        try
        {
            logger.LogInformation("Extracting text from PDF: {PdfPath} using language: {Language}", pdfPath, language);

            if (string.IsNullOrWhiteSpace(pdfPath))
                return ArchiveErrors.InvalidInput;

            if (!System.IO.File.Exists(pdfPath))
                return ArchiveErrors.InvalidInput;

            var extractedText = await ocrGenerator.ExtractTextFromPdfAsync(pdfPath, language);

            logger.LogInformation("Successfully extracted text from PDF: {PdfPath}", pdfPath);
            return extractedText;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting text from PDF: {PdfPath}", pdfPath);
            return ArchiveErrors.InternalServerError;
        }
    }
}
