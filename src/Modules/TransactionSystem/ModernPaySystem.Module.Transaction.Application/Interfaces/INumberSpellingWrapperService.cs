using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Transaction.Application.Interfaces;

public interface INumberSpellingWrapperService
{
    Result<string> ConvertNumberToArabicWords(decimal number);

    Result<string> ConvertNumberToArabicWords(int number);

    Result<string> ConvertNumberToArabicWords(long number);
}
