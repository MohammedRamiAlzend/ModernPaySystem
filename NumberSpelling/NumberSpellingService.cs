namespace NumberSpelling;

public class NumberSpellingService : INumberSpellingService
{
    public async Task<string> ConvertNumberToArabicWordsAsync(decimal number)
    {
        // Placeholder implementation - in a real scenario, this would call the actual NumberToArabicText library
        return await Task.Run(() => $"Number {number} in Arabic words");
    }

    public async Task<string> ConvertNumberToArabicWordsAsync(double number)
    {
        // Placeholder implementation - in a real scenario, this would call the actual NumberToArabicText library
        return await Task.Run(() => $"Number {number} in Arabic words");
    }

    public async Task<string> ConvertNumberToArabicWordsAsync(int number)
    {
        // Placeholder implementation - in a real scenario, this would call the actual NumberToArabicText library
        return await Task.Run(() => $"Number {number} in Arabic words");
    }

    public async Task<string> ConvertNumberToArabicWordsAsync(long number)
    {
        // Placeholder implementation - in a real scenario, this would call the actual NumberToArabicText library
        return await Task.Run(() => $"Number {number} in Arabic words");
    }
}