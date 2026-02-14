using ModernPaySystem.Domain.Commons;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Number Spelling service for converting numbers to Arabic words.
/// </summary>
public interface INumberSpellingWrapperService
{
    /// <summary>
    /// Convert a decimal number to Arabic words.
    /// </summary>
    Result<string> ConvertNumberToArabicWords(decimal number);

    /// <summary>
    /// Convert an integer number to Arabic words.
    /// </summary>
    Result<string> ConvertNumberToArabicWords(int number);

    /// <summary>
    /// Convert a long number to Arabic words.
    /// </summary>
    Result<string> ConvertNumberToArabicWords(long number);
}