using WordFileTest.NumberToArabicText;

namespace NumberSpelling;

public class NumberSpellingService : INumberSpellingService
{
    public string ConvertNumberToArabicWords(decimal number)
    {
        return ArabicWordConverter.ToArabicWord(number).ToString()!;
    }

    public string ConvertNumberToArabicWords(int number)
    {
        return ArabicWordConverter.ToArabicWord(number).ToString()!;
    }

    public string ConvertNumberToArabicWords(long number)
    {
        return ArabicWordConverter.ToArabicWord(number).ToString()!;
    }
}
