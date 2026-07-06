using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Archive.Api.Extensions;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Api.Controllers;

[ApiController]
[Route("api/ocr")]
[Authorize]
public class OcrController(
    IOcrService ocrService,
    ILogger<OcrController> logger) : ControllerBase
{
    [HttpPost("extract-text-from-image")]
    [EndpointPermission("ocr.extract-text-from-image", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> ExtractTextFromImage([FromForm] IFormFile imageFile, [FromQuery] string language = "eng")
    {
        if (imageFile == null || imageFile.Length == 0)
            return BadRequest("No image file provided");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif" };
        var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Invalid file type. Only image files are allowed.");

        try
        {
            var tempFilePath = Path.GetTempFileName() + fileExtension;
            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var result = await ocrService.ExtractTextFromImageAsync(tempFilePath, language);

            if (System.IO.File.Exists(tempFilePath))
                System.IO.File.Delete(tempFilePath);

            if (result.IsError)
                return result.ToActionResult();

            return Ok(new OcrResponse
            {
                Success = true,
                ExtractedText = result.Value,
                Language = language,
                FileName = imageFile.FileName
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting text from image: {FileName}", imageFile.FileName);
            return StatusCode(500, new OcrErrorResponse
            {
                Success = false,
                Error = "Failed to process image",
                Details = ex.Message
            });
        }
    }

    [HttpPost("extract-text-from-pdf")]
    [EndpointPermission("ocr.extract-text-from-pdf", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> ExtractTextFromPdf([FromForm] IFormFile pdfFile, [FromQuery] string language = "eng")
    {
        if (pdfFile == null || pdfFile.Length == 0)
            return BadRequest("No PDF file provided");

        var fileExtension = Path.GetExtension(pdfFile.FileName).ToLower();
        if (fileExtension != ".pdf")
            return BadRequest("Invalid file type. Only PDF files are allowed.");

        try
        {
            var tempFilePath = Path.GetTempFileName() + ".pdf";
            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            var result = await ocrService.ExtractTextFromPdfAsync(tempFilePath, language);

            if (System.IO.File.Exists(tempFilePath))
                System.IO.File.Delete(tempFilePath);

            if (result.IsError)
                return result.ToActionResult();

            return Ok(new OcrResponse
            {
                Success = true,
                ExtractedText = result.Value,
                Language = language,
                FileName = pdfFile.FileName
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting text from PDF: {FileName}", pdfFile.FileName);
            return StatusCode(500, new OcrErrorResponse
            {
                Success = false,
                Error = "Failed to process PDF",
                Details = ex.Message
            });
        }
    }

    [HttpGet("supported-languages")]
    [EndpointPermission("ocr.supported-languages", SubSystem.Archiving, PermissionType.Read)]
    public IActionResult GetSupportedLanguages()
    {
        var languages = new[]
        {
            new OcrLanguage { Code = "ara", Name = "Arabic" },
            new OcrLanguage { Code = "eng", Name = "English" },
            new OcrLanguage { Code = "fra", Name = "French" },
            new OcrLanguage { Code = "deu", Name = "German" },
            new OcrLanguage { Code = "spa", Name = "Spanish" },
            new OcrLanguage { Code = "ita", Name = "Italian" },
            new OcrLanguage { Code = "por", Name = "Portuguese" },
            new OcrLanguage { Code = "rus", Name = "Russian" },
            new OcrLanguage { Code = "chi_sim", Name = "Chinese Simplified" },
            new OcrLanguage { Code = "jpn", Name = "Japanese" }
        };

        return Ok(new SupportedLanguagesResponse
        {
            Success = true,
            Languages = languages,
            DefaultLanguage = "eng"
        });
    }
}

public class OcrResponse
{
    public bool Success { get; set; }
    public string? ExtractedText { get; set; }
    public string? Language { get; set; }
    public string? FileName { get; set; }
}

public class OcrErrorResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Details { get; set; }
}

public class OcrLanguage
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}

public class SupportedLanguagesResponse
{
    public bool Success { get; set; }
    public OcrLanguage[]? Languages { get; set; }
    public string? DefaultLanguage { get; set; }
}
