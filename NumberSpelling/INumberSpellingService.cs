namespace NumberSpelling;

public interface INumberSpellingService
{
    Task<string> ConvertNumberToArabicWordsAsync(decimal number);
    Task<string> ConvertNumberToArabicWordsAsync(double number);
    Task<string> ConvertNumberToArabicWordsAsync(int number);
    Task<string> ConvertNumberToArabicWordsAsync(long number);
}