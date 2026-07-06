using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Commons;
using NumberSpelling;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class NumberSpellingWrapperService(
    INumberSpellingService numberSpellingService,
    ILogger<NumberSpellingWrapperService> logger) : INumberSpellingWrapperService
{
    public Result<string> ConvertNumberToArabicWords(decimal number)
    {
        try
        {
            logger.LogInformation("Converting decimal number {Number} to Arabic words", number);
            var result = numberSpellingService.ConvertNumberToArabicWords(number);
            logger.LogInformation("Successfully converted decimal number {Number} to Arabic words", number);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error converting decimal number {Number} to Arabic words", number);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public Result<string> ConvertNumberToArabicWords(int number)
    {
        try
        {
            logger.LogInformation("Converting integer number {Number} to Arabic words", number);
            var result = numberSpellingService.ConvertNumberToArabicWords(number);
            logger.LogInformation("Successfully converted integer number {Number} to Arabic words", number);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error converting integer number {Number} to Arabic words", number);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public Result<string> ConvertNumberToArabicWords(long number)
    {
        try
        {
            logger.LogInformation("Converting long number {Number} to Arabic words", number);
            var result = numberSpellingService.ConvertNumberToArabicWords(number);
            logger.LogInformation("Successfully converted long number {Number} to Arabic words", number);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error converting long number {Number} to Arabic words", number);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }
}
