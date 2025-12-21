using AlgerianChequeFiller.Models;

namespace AlgerianChequeFiller.Services.NumberToWords;

/// <summary>
/// Factory for creating number-to-words converters.
/// </summary>
public static class ConverterFactory
{
    public static INumberToWordsConverter Create(Language language) => language switch
    {
        Language.French => new FrenchConverter(),
        Language.Arabic => new ArabicConverter(),
        Language.English => new EnglishConverter(),
        _ => new FrenchConverter()
    };
}
