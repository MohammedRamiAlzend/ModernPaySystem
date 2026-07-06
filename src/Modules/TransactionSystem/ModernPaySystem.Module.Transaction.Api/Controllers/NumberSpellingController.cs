using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Transaction.Api.Extensions;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Api.Controllers;

[ApiController]
[Route("api/transaction/[controller]")]
[Authorize]
public class NumberSpellingController(INumberSpellingWrapperService numberSpellingService, ILogger<NumberSpellingController> logger) : ControllerBase
{
    [HttpPost("convert-decimal")]
    [EndpointPermission("number-spelling.convert-decimal", SubSystem.TransactionSystem, PermissionType.Read)]
    public IActionResult ConvertDecimal(decimal number)
    {
        logger.LogInformation("Converting decimal number to Arabic words: {Number}", number);
        var result = numberSpellingService.ConvertNumberToArabicWords(number);
        return result.ToActionResult();
    }

    [HttpPost("convert-int")]
    [EndpointPermission("number-spelling.convert-int", SubSystem.TransactionSystem, PermissionType.Read)]
    public IActionResult ConvertInt(int number)
    {
        logger.LogInformation("Converting integer number to Arabic words: {Number}", number);
        var result = numberSpellingService.ConvertNumberToArabicWords(number);
        return result.ToActionResult();
    }

    [HttpPost("convert-long")]
    [EndpointPermission("number-spelling.convert-long", SubSystem.TransactionSystem, PermissionType.Read)]
    public IActionResult ConvertLong(long number)
    {
        logger.LogInformation("Converting long number to Arabic words: {Number}", number);
        var result = numberSpellingService.ConvertNumberToArabicWords(number);
        return result.ToActionResult();
    }
}
