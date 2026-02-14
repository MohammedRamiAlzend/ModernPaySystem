namespace NumberSpelling;

public interface INumberSpellingService
{
    string ConvertNumberToArabicWords(decimal number);
    string ConvertNumberToArabicWords(int number);
    string ConvertNumberToArabicWords(long number);
}