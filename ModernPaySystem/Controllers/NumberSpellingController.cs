namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NumberSpellingController : ControllerBase
{
    private readonly INumberSpellingWrapperService _numberSpellingService;
    private readonly ILogger<NumberSpellingController> _logger;

    public NumberSpellingController(INumberSpellingWrapperService numberSpellingService, ILogger<NumberSpellingController> logger)
    {
        _numberSpellingService = numberSpellingService;
        _logger = logger;
    }

    [HttpPost("convert-decimal-to-arabic")]
    [EndpointPermission("numberspelling.convert-decimal", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ConvertDecimalToArabic([FromBody] decimal number)
    {
        _logger.LogInformation("Converting decimal number {Number} to Arabic words", number);
        var result = _numberSpellingService.ConvertNumberToArabicWords(number);
        return result.ToActionResult();
    }

    [HttpPost("convert-int-to-arabic")]
    [EndpointPermission("numberspelling.convert-int", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ConvertIntToArabic([FromBody] int number)
    {
        _logger.LogInformation("Converting integer number {Number} to Arabic words", number);
        var result = _numberSpellingService.ConvertNumberToArabicWords(number);
        return result.ToActionResult();
    }

    [HttpPost("convert-long-to-arabic")]
    [EndpointPermission("numberspelling.convert-long", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ConvertLongToArabic([FromBody] long number)
    {
        _logger.LogInformation("Converting long number {Number} to Arabic words", number);
        var result = _numberSpellingService.ConvertNumberToArabicWords(number);
        return result.ToActionResult();
    }
}
