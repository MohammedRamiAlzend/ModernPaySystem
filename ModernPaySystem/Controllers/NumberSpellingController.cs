using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for Number Spelling operations
/// Provides conversion of numbers to Arabic words
/// </summary>
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

    /// <summary>
    /// Convert a decimal number to Arabic words
    /// </summary>
    [HttpPost("convert-decimal-to-arabic")]
    [EndpointPermission("numberspelling.convert-decimal", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ConvertDecimalToArabic([FromBody] decimal number)
    {
        _logger.LogInformation("Converting decimal number {Number} to Arabic words", number);
        var result = await _numberSpellingService.ConvertNumberToArabicWordsAsync(number);
        return result.ToActionResult();
    }

    /// <summary>
    /// Convert a double number to Arabic words
    /// </summary>
    [HttpPost("convert-double-to-arabic")]
    [EndpointPermission("numberspelling.convert-double", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ConvertDoubleToArabic([FromBody] double number)
    {
        _logger.LogInformation("Converting double number {Number} to Arabic words", number);
        var result = await _numberSpellingService.ConvertNumberToArabicWordsAsync(number);
        return result.ToActionResult();
    }

    /// <summary>
    /// Convert an integer number to Arabic words
    /// </summary>
    [HttpPost("convert-int-to-arabic")]
    [EndpointPermission("numberspelling.convert-int", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ConvertIntToArabic([FromBody] int number)
    {
        _logger.LogInformation("Converting integer number {Number} to Arabic words", number);
        var result = await _numberSpellingService.ConvertNumberToArabicWordsAsync(number);
        return result.ToActionResult();
    }

    /// <summary>
    /// Convert a long number to Arabic words
    /// </summary>
    [HttpPost("convert-long-to-arabic")]
    [EndpointPermission("numberspelling.convert-long", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> ConvertLongToArabic([FromBody] long number)
    {
        _logger.LogInformation("Converting long number {Number} to Arabic words", number);
        var result = await _numberSpellingService.ConvertNumberToArabicWordsAsync(number);
        return result.ToActionResult();
    }
}