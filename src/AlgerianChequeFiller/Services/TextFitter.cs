using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace AlgerianChequeFiller.Services;

/// <summary>
/// Result of text fitting operation.
/// </summary>
public record FitResult(List<string> Lines, double FontSize, bool Success);

/// <summary>
/// Fits text into one or two lines with auto font sizing.
/// </summary>
public class TextFitter
{
    private readonly double _pixelsPerDip;

    public TextFitter(double pixelsPerDip = 1.0)
    {
        _pixelsPerDip = pixelsPerDip;
    }

    /// <summary>
    /// Fit text into the provided rectangles.
    /// </summary>
    public FitResult Fit(
        string text,
        Rect line1Rect,
        Rect line2Rect,
        string fontFamily,
        double minFontSize,
        double maxFontSize,
        bool isRtl)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new FitResult(new List<string> { "" }, maxFontSize, true);
        }

        // Step 1: Try single line at max font size in L1
        if (TextFits(text, line1Rect, fontFamily, maxFontSize))
        {
            return new FitResult(new List<string> { text }, maxFontSize, true);
        }

        // Step 2: Try splitting into two lines
        var words = SplitWords(text, isRtl);

        for (double fontSize = maxFontSize; fontSize >= minFontSize; fontSize -= 0.5)
        {
            var bestSplit = FindBestSplit(words, line1Rect, line2Rect, fontFamily, fontSize);
            if (bestSplit != null)
            {
                return new FitResult(bestSplit, fontSize, true);
            }
        }

        // Step 3: Force fit at minimum size
        var forcedSplit = ForceSplit(words, line1Rect, line2Rect, fontFamily, minFontSize);
        return new FitResult(forcedSplit, minFontSize, false);
    }

    private bool TextFits(string text, Rect rect, string fontFamily, double fontSize)
    {
        var formattedText = CreateFormattedText(text, fontFamily, fontSize);
        return formattedText.Width <= rect.Width && formattedText.Height <= rect.Height;
    }

    private FormattedText CreateFormattedText(string text, string fontFamily, double fontSize)
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily),
            fontSize,
            Brushes.Black,
            _pixelsPerDip);
    }

    private static string[] SplitWords(string text, bool isRtl)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    private List<string>? FindBestSplit(
        string[] words, Rect line1, Rect line2, string fontFamily, double fontSize)
    {
        // Try different split points
        for (int i = 1; i < words.Length; i++)
        {
            var l1Text = string.Join(" ", words.Take(i));
            var l2Text = string.Join(" ", words.Skip(i));

            if (TextFits(l1Text, line1, fontFamily, fontSize) &&
                TextFits(l2Text, line2, fontFamily, fontSize))
            {
                return new List<string> { l1Text, l2Text };
            }
        }
        return null;
    }

    private List<string> ForceSplit(
        string[] words, Rect line1, Rect line2, string fontFamily, double fontSize)
    {
        // Split roughly in half
        int midPoint = words.Length / 2;
        if (midPoint == 0) midPoint = 1;

        var l1Text = string.Join(" ", words.Take(midPoint));
        var l2Text = string.Join(" ", words.Skip(midPoint));

        return new List<string> { l1Text, l2Text };
    }
}
