using System.Windows;

namespace AlgerianChequeFiller.Services;

/// <summary>
/// Coordinate conversion utilities between millimeters and WPF DIPs.
/// </summary>
public static class CoordinateConverter
{
    private const double MmPerInch = 25.4;
    private const double WpfDpi = 96.0;

    /// <summary>
    /// Convert millimeters to WPF DIPs.
    /// </summary>
    public static double MmToDip(double mm)
    {
        return mm / MmPerInch * WpfDpi;
    }

    /// <summary>
    /// Convert WPF DIPs to millimeters.
    /// </summary>
    public static double DipToMm(double dip)
    {
        return dip / WpfDpi * MmPerInch;
    }

    /// <summary>
    /// Convert mm to printer pixels at given DPI.
    /// </summary>
    public static double MmToPixels(double mm, double dpi)
    {
        return mm / MmPerInch * dpi;
    }

    /// <summary>
    /// Convert a size from mm to WPF.
    /// </summary>
    public static Size MmToWpfSize(double widthMm, double heightMm)
    {
        return new Size(MmToDip(widthMm), MmToDip(heightMm));
    }

    /// <summary>
    /// Convert a point from mm to WPF.
    /// </summary>
    public static Point MmToWpfPoint(double xMm, double yMm)
    {
        return new Point(MmToDip(xMm), MmToDip(yMm));
    }
}
