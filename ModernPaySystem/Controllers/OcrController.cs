using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for OCR operations
/// Provides text extraction from images and PDFs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<OcrController> _logger;

    public OcrController(IOcrService ocrService, ILogger<OcrController> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// Extract text from an image file
    /// </summary>
    [HttpPost("extract-text-from-image")]
    [EndpointPermission("ocr.extract-text-from-image", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ExtractTextFromImage([FromQuery] string imagePath, [FromQuery] string language = "eng")
    {
        _logger.LogInformation("Extracting text from image: {ImagePath} with language: {Language}", imagePath, language);
        var result = await _ocrService.ExtractTextFromImageAsync(imagePath, language);
        return result.ToActionResult();
    }

    /// <summary>
    /// Extract text from a PDF file
    /// </summary>
    [HttpPost("extract-text-from-pdf")]
    [EndpointPermission("ocr.extract-text-from-pdf", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ExtractTextFromPdf([FromQuery] string pdfPath, [FromQuery] string language = "eng")
    {
        _logger.LogInformation("Extracting text from PDF: {PdfPath} with language: {Language}", pdfPath, language);
        var result = await _ocrService.ExtractTextFromPdfAsync(pdfPath, language);
        return result.ToActionResult();
    }
}