namespace AlgerianChequeFiller.Services.NumberToWords;

/// <summary>
/// Interface for number-to-words converters.
/// </summary>
public interface INumberToWordsConverter
{
    /// <summary>
    /// Convert a decimal amount to words.
    /// </summary>
    /// <param name="amount">Amount in dinars</param>
    /// <returns>Amount in words</returns>
    string Convert(decimal amount);

    /// <summary>
    /// Whether the language is RTL.
    /// </summary>
    bool IsRtl { get; }
}
