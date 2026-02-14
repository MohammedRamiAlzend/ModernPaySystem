using Microsoft.Extensions.Logging;
using ModernPaySystem.Domain.Commons;
using NumberSpelling;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Number Spelling service for converting numbers to Arabic words.
/// </summary>
public class NumberSpellingWrapperService : INumberSpellingWrapperService
{
    private readonly INumberSpellingService _numberSpellingService;
    private readonly ILogger<NumberSpellingWrapperService> _logger;

    public NumberSpellingWrapperService(INumberSpellingService numberSpellingService, ILogger<NumberSpellingWrapperService> logger)
    {
        _numberSpellingService = numberSpellingService;
        _logger = logger;
    }

    public async Task<Result<string>> ConvertNumberToArabicWordsAsync(decimal number)
    {
        try
        {
            _logger.LogInformation("Converting decimal number {Number} to Arabic words", number);
            
            var result = await _numberSpellingService.ConvertNumberToArabicWordsAsync(number);
            
            _logger.LogInformation("Successfully converted decimal number {Number} to Arabic words", number);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting decimal number {Number} to Arabic words", number);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<string>> ConvertNumberToArabicWordsAsync(double number)
    {
        try
        {
            _logger.LogInformation("Converting double number {Number} to Arabic words", number);
            
            var result = await _numberSpellingService.ConvertNumberToArabicWordsAsync(number);
            
            _logger.LogInformation("Successfully converted double number {Number} to Arabic words", number);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting double number {Number} to Arabic words", number);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<string>> ConvertNumberToArabicWordsAsync(int number)
    {
        try
        {
            _logger.LogInformation("Converting integer number {Number} to Arabic words", number);
            
            var result = await _numberSpellingService.ConvertNumberToArabicWordsAsync(number);
            
            _logger.LogInformation("Successfully converted integer number {Number} to Arabic words", number);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting integer number {Number} to Arabic words", number);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<string>> ConvertNumberToArabicWordsAsync(long number)
    {
        try
        {
            _logger.LogInformation("Converting long number {Number} to Arabic words", number);
            
            var result = await _numberSpellingService.ConvertNumberToArabicWordsAsync(number);
            
            _logger.LogInformation("Successfully converted long number {Number} to Arabic words", number);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting long number {Number} to Arabic words", number);
            return ApplicationErrors.InternalServerError;
        }
    }
}