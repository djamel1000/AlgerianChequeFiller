using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AlgerianChequeFiller.Models;

namespace AlgerianChequeFiller.Services;

/// <summary>
/// Handles printing operations.
/// </summary>
public class PrintService
{
    /// <summary>
    /// Print a cheque using the system print dialog.
    /// </summary>
    public bool Print(ChequeData data, ChequeTemplate template, bool testMode)
    {
        var printDialog = new PrintDialog();

        // Set custom page size
        try
        {
            var ticket = printDialog.PrintTicket;
            ticket.PageMediaSize = new PageMediaSize(
                PageMediaSizeName.Unknown,
                template.ChequeSizeMm.Width / 25.4 * 96,  // Convert to 1/96 inch units
                template.ChequeSizeMm.Height / 25.4 * 96
            );

            // Disable scaling
            ticket.PageScalingFactor = 100;

            // Set orientation
            ticket.PageOrientation = PageOrientation.Landscape;
        }
        catch
        {
            // Some printers may not support custom page size
        }

        if (printDialog.ShowDialog() == true)
        {
            var visual = RenderCheque(data, template, testMode);
            var description = testMode ? "Test d'alignement" : "Impression de chèque";
            printDialog.PrintVisual(visual, description);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Create a DrawingVisual for preview or printing.
    /// </summary>
    public DrawingVisual RenderCheque(ChequeData data, ChequeTemplate template, bool testMode, double pixelsPerDip = 1.0)
    {
        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();

        // Draw white background for preview
        var pageRect = new Rect(0, 0,
            CoordinateConverter.MmToDip(template.ChequeSizeMm.Width),
            CoordinateConverter.MmToDip(template.ChequeSizeMm.Height));
        dc.DrawRectangle(Brushes.White, null, pageRect);

        var renderer = new ChequeRenderer(dc, template, testMode, pixelsPerDip);
        renderer.Render(data);

        return visual;
    }

    /// <summary>
    /// Check if any printers are available.
    /// </summary>
    public static bool HasPrinters()
    {
        try
        {
            var server = new LocalPrintServer();
            return server.GetPrintQueues().Any();
        }
        catch
        {
            return false;
        }
    }
}
