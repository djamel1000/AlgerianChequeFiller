using System.Windows;

namespace AlgerianChequeFiller.Models;

/// <summary>
/// Represents a field rectangle on the cheque template.
/// All values are in millimeters.
/// </summary>
public record FieldRect(double X, double Y, double W, double H)
{
    /// <summary>
    /// Text alignment within the field.
    /// </summary>
    public string Alignment { get; init; } = "Left";

    /// <summary>
    /// Convert this field rect to WPF rect in DIPs.
    /// </summary>
    public Rect ToWpfRect(double offsetX = 0, double offsetY = 0)
    {
        const double MmPerInch = 25.4;
        const double WpfDpi = 96.0;

        double xDip = (X + offsetX) / MmPerInch * WpfDpi;
        double yDip = (Y + offsetY) / MmPerInch * WpfDpi;
        double wDip = W / MmPerInch * WpfDpi;
        double hDip = H / MmPerInch * WpfDpi;

        return new Rect(xDip, yDip, wDip, hDip);
    }
}

/// <summary>
/// Field identifiers for cheque template.
/// </summary>
public enum FieldId
{
    AmountNumeric,
    AmountWordsL1,
    AmountWordsL2,
    Beneficiary,
    Place,
    Date
}
