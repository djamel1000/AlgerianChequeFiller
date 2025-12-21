using System.Globalization;

namespace AlgerianChequeFiller.Models;

/// <summary>
/// Holds the user-entered cheque data.
/// </summary>
public class ChequeData
{
    /// <summary>
    /// Amount in Algerian Dinars.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Name of the beneficiary.
    /// </summary>
    public string Beneficiary { get; set; } = string.Empty;

    /// <summary>
    /// Place/city where the cheque is issued.
    /// </summary>
    public string Place { get; set; } = string.Empty;

    /// <summary>
    /// Date of the cheque.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// Language for amount-in-words conversion.
    /// </summary>
    public Language Language { get; set; } = Language.French;

    /// <summary>
    /// Split amount into dinars and centimes parts.
    /// </summary>
    public (long Dinars, int Centimes) GetAmountParts()
    {
        var dinars = (long)Math.Floor(Amount);
        var centimes = (int)Math.Round((Amount - dinars) * 100);
        return (dinars, centimes);
    }

    /// <summary>
    /// Formatted amount string (e.g., "1 234,50 DA").
    /// </summary>
    public string FormattedAmount => Amount.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")) + " DA";

    /// <summary>
    /// Formatted date string (dd/MM/yyyy).
    /// </summary>
    public string FormattedDate => Date.ToString("dd/MM/yyyy");
}
