using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Application.Interfaces;

public interface IOcrService
{
    Task<Result<string>> ExtractTextFromImageAsync(string imagePath, string language = "eng");
    Task<Result<string>> ExtractTextFromPdfAsync(string pdfPath, string language = "eng");
}
