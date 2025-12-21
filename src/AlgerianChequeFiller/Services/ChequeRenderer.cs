using System.Globalization;
using System.Windows;
using System.Windows.Media;
using AlgerianChequeFiller.Models;
using AlgerianChequeFiller.Services.NumberToWords;

namespace AlgerianChequeFiller.Services;

/// <summary>
/// Renders cheque content using DrawingContext.
/// </summary>
public class ChequeRenderer
{
    private readonly DrawingContext _dc;
    private readonly ChequeTemplate _template;
    private readonly bool _testMode;
    private readonly double _pixelsPerDip;

    private static readonly Dictionary<FieldId, Color> FieldColors = new()
    {
        [FieldId.AmountNumeric] = Colors.Blue,
        [FieldId.AmountWordsL1] = Colors.Green,
        [FieldId.AmountWordsL2] = Colors.Green,
        [FieldId.Beneficiary] = Colors.Purple,
        [FieldId.Place] = Colors.Orange,
        [FieldId.Date] = Colors.Red
    };

    public ChequeRenderer(DrawingContext dc, ChequeTemplate template, bool testMode, double pixelsPerDip = 1.0)
    {
        _dc = dc;
        _template = template;
        _testMode = testMode;
        _pixelsPerDip = pixelsPerDip;
    }

    /// <summary>
    /// Render the cheque content.
    /// </summary>
    public void Render(ChequeData data)
    {
        if (_testMode)
        {
            DrawDebugOverlay();
        }

        // Draw each field
        DrawField(FieldId.AmountNumeric, data.FormattedAmount, FlowDirection.RightToLeft);
        DrawField(FieldId.Beneficiary, data.Beneficiary, FlowDirection.LeftToRight);
        DrawField(FieldId.Place, data.Place, FlowDirection.LeftToRight);
        DrawField(FieldId.Date, data.FormattedDate, FlowDirection.LeftToRight);

        // Draw amount in words
        DrawAmountInWords(data);
    }

    private void DrawDebugOverlay()
    {
        // Draw page border
        var pageRect = new Rect(0, 0,
            CoordinateConverter.MmToDip(_template.ChequeSizeMm.Width),
            CoordinateConverter.MmToDip(_template.ChequeSizeMm.Height));
        _dc.DrawRectangle(null, new Pen(Brushes.Gray, 0.5), pageRect);

        // Draw each field rectangle with label
        foreach (var (fieldName, fieldRect) in _template.Fields)
        {
            if (!Enum.TryParse<FieldId>(fieldName, out var fieldId))
                continue;

            var rect = fieldRect.ToWpfRect(_template.GlobalOffsetMm.X, _template.GlobalOffsetMm.Y);
            var color = FieldColors.GetValueOrDefault(fieldId, Colors.Gray);
            var brush = new SolidColorBrush(color);

            // Rectangle
            _dc.DrawRectangle(null, new Pen(brush, 0.5), rect);

            // Crosshair at center
            var center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            _dc.DrawLine(new Pen(brush, 0.3),
                new Point(center.X - 2, center.Y), new Point(center.X + 2, center.Y));
            _dc.DrawLine(new Pen(brush, 0.3),
                new Point(center.X, center.Y - 2), new Point(center.X, center.Y + 2));

            // Label
            var label = new FormattedText(fieldName, CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 6, brush, _pixelsPerDip);
            _dc.DrawText(label, new Point(rect.X, rect.Y - 8));
        }
    }

    private void DrawField(FieldId fieldId, string text, FlowDirection flowDirection)
    {
        var fieldRect = _template.GetField(fieldId);
        if (fieldRect == null || string.IsNullOrEmpty(text))
            return;

        var rect = fieldRect.ToWpfRect(_template.GlobalOffsetMm.X, _template.GlobalOffsetMm.Y);
        var fontFamily = _template.Fonts.LatinFamily;
        var fontSize = _template.Fonts.MaxSize;

        var formattedText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            flowDirection,
            new Typeface(fontFamily),
            fontSize,
            Brushes.Black,
            _pixelsPerDip);

        formattedText.MaxTextWidth = rect.Width;
        formattedText.MaxTextHeight = rect.Height;

        var x = fieldRect.Alignment == "Right" ? rect.Right - formattedText.Width : rect.X;
        _dc.DrawText(formattedText, new Point(x, rect.Y));
    }

    private void DrawAmountInWords(ChequeData data)
    {
        var converter = ConverterFactory.Create(data.Language);
        var amountInWords = converter.Convert(data.Amount);

        var line1Rect = _template.GetField(FieldId.AmountWordsL1);
        var line2Rect = _template.GetField(FieldId.AmountWordsL2);

        if (line1Rect == null || line2Rect == null)
            return;

        var rect1 = line1Rect.ToWpfRect(_template.GlobalOffsetMm.X, _template.GlobalOffsetMm.Y);
        var rect2 = line2Rect.ToWpfRect(_template.GlobalOffsetMm.X, _template.GlobalOffsetMm.Y);

        var fontFamily = converter.IsRtl ? _template.Fonts.ArabicFamily : _template.Fonts.LatinFamily;
        var flowDirection = converter.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        var textFitter = new TextFitter(_pixelsPerDip);
        var fitResult = textFitter.Fit(
            amountInWords,
            rect1,
            rect2,
            fontFamily,
            _template.Fonts.MinSize,
            _template.Fonts.MaxSize,
            converter.IsRtl);

        // Draw lines
        if (fitResult.Lines.Count > 0 && !string.IsNullOrEmpty(fitResult.Lines[0]))
        {
            var text1 = new FormattedText(
                fitResult.Lines[0],
                CultureInfo.CurrentCulture,
                flowDirection,
                new Typeface(fontFamily),
                fitResult.FontSize,
                Brushes.Black,
                _pixelsPerDip);

            text1.MaxTextWidth = rect1.Width;
            _dc.DrawText(text1, new Point(rect1.X, rect1.Y));
        }

        if (fitResult.Lines.Count > 1 && !string.IsNullOrEmpty(fitResult.Lines[1]))
        {
            var text2 = new FormattedText(
                fitResult.Lines[1],
                CultureInfo.CurrentCulture,
                flowDirection,
                new Typeface(fontFamily),
                fitResult.FontSize,
                Brushes.Black,
                _pixelsPerDip);

            text2.MaxTextWidth = rect2.Width;
            _dc.DrawText(text2, new Point(rect2.X, rect2.Y));
        }
    }
}
