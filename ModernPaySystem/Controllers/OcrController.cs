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
    public async Task<IActionResult> ExtractTextFromImage([FromForm] IFormFile imageFile, [FromQuery] string language = "eng")
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest("No image file provided");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif" };
        var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest("Invalid file type. Only image files are allowed.");
        }

        try
        {
            // Save the uploaded file temporarily
            var tempFilePath = Path.GetTempFileName() + fileExtension;
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Extract text using OCR
            var result = await _ocrService.ExtractTextFromImageAsync(tempFilePath, language);
            
            // Clean up temporary file
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }

            if (result.IsError)
            {
                return result.ToActionResult();
            }

            var response = new OcrResponse
            {
                Success = true,
                ExtractedText = result.Value,
                Language = language,
                FileName = imageFile.FileName
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from image: {FileName}", imageFile.FileName);
            
            var errorResponse = new OcrErrorResponse
            {
                Success = false,
                Error = "Failed to process image",
                Details = ex.Message
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Extract text from a PDF file
    /// </summary>
    [HttpPost("extract-text-from-pdf")]
    [EndpointPermission("ocr.extract-text-from-pdf", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ExtractTextFromPdf([FromForm] IFormFile pdfFile, [FromQuery] string language = "eng")
    {
        if (pdfFile == null || pdfFile.Length == 0)
        {
            return BadRequest("No PDF file provided");
        }

        // Validate file type
        var fileExtension = Path.GetExtension(pdfFile.FileName).ToLower();
        if (fileExtension != ".pdf")
        {
            return BadRequest("Invalid file type. Only PDF files are allowed.");
        }

        try
        {
            // Save the uploaded file temporarily
            var tempFilePath = Path.GetTempFileName() + ".pdf";
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
            }

            // Extract text using OCR
            var result = await _ocrService.ExtractTextFromPdfAsync(tempFilePath, language);

            // Clean up temporary file
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }

            if (result.IsError)
            {
                return result.ToActionResult();
            }

            var response = new OcrResponse
            {
                Success = true,
                ExtractedText = result.Value,
                Language = language,
                FileName = pdfFile.FileName
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF: {FileName}", pdfFile.FileName);
            
            var errorResponse = new OcrErrorResponse
            {
                Success = false,
                Error = "Failed to process PDF",
                Details = ex.Message
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Get supported languages for OCR
    /// </summary>
    [HttpGet("supported-languages")]
    [EndpointPermission("ocr.supported-languages", SubSystem.TransactionSystem, PermissionType.Read)]
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

        var response = new SupportedLanguagesResponse
        {
            Success = true,
            Languages = languages,
            DefaultLanguage = "eng"
        };

        return Ok(response);
    }
}

// Response DTOs for OpenAPI compatibility
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