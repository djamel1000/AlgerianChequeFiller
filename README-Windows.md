# Algerian Cheque Filler (Windows)

A Windows desktop application (supports windows 7 and later)for printing user-entered data onto pre-printed Algerian cheques with precise field positioning, multi-language amount-in-words conversion, and printer calibration support.

---

## Table of Contents

1. [Overview](#overview)
2. [Technology Stack](#technology-stack)
3. [Features](#features)
4. [Installation](#installation)
5. [Project Structure](#project-structure)
6. [Data Models](#data-models)
7. [Default Template JSON](#default-template-json)
8. [Coordinate System & Conversion](#coordinate-system--conversion)
9. [Number-to-Words Converters](#number-to-words-converters)
10. [Text Fitting Algorithm](#text-fitting-algorithm)
11. [Printing System](#printing-system)
12. [User Interface](#user-interface)
13. [Testing](#testing)
14. [Legal Disclaimer](#legal-disclaimer)

---

## Overview

**Algerian Cheque Filler** allows users to:
- Enter cheque details (amount, beneficiary, place, date)
- Convert amounts to words in French, Arabic, or English
- Preview the cheque layout with precise field positioning
- Print directly onto pre-printed cheque stock
- Calibrate printer offsets for perfect alignment

### Supported Languages
| Language | Role | Amount Format |
|----------|------|---------------|
| French | Primary | "Mille deux cent trente-quatre dinars et cinquante-six centimes" |
| Arabic | Secondary | "ألف ومئتان وأربعة وثلاثون دينارًا جزائريًا وست وخمسون سنتيمًا" |
| English | Secondary | "One thousand two hundred thirty-four Algerian dinars and fifty-six centimes" |

---

## Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | .NET 8.0 |
| **UI** | WPF (Windows Presentation Foundation) |
| **Architecture** | MVVM (Model-View-ViewModel) |
| **Printing** | System.Printing + PrintDialog |
| **Preview** | DrawingVisual / XPS rendering |
| **PDF Export** | PdfSharp (optional) |
| **JSON** | System.Text.Json |
| **Testing** | xUnit + FluentAssertions |

### Why WPF?
- Mature, stable printing API with full control over page layout
- Device-independent units (DIPs) for consistent rendering
- Excellent RTL support for Arabic text
- Rich text rendering with FormattedText

---

## Features

### Core Features
- [x] Amount input with automatic formatting (DZD currency)
- [x] Real-time amount-to-words conversion (FR/AR/EN)
- [x] Two-line text fitting with auto font sizing
- [x] Live cheque preview
- [x] Direct printing to physical printers
- [x] Print to PDF support

### Template System
- [x] Editable field rectangles (in millimeters)
- [x] Global X/Y offset for printer calibration
- [x] Multiple template support
- [x] JSON-based template storage

### Calibration
- [x] Test print mode with bounding boxes and crosshairs
- [x] Step-by-step calibration wizard
- [x] Per-printer offset adjustments

### Safety
- [x] First-launch legal disclaimer (blocking)
- [x] Print confirmation dialog
- [x] Blank paper test recommendation

---

## Installation

### Prerequisites
- Windows 10 version 1809 or later
- .NET 8.0 Desktop Runtime
- A printer (for printing)

### Build from Source
```powershell
# Clone repository
git clone https://github.com/your-org/algerian-cheque-filler-windows.git
cd algerian-cheque-filler-windows

# Restore and build
dotnet restore
dotnet build --configuration Release

# Run
dotnet run --project src/AlgerianChequeFiller
```

### Publish
```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

---

## Project Structure

```
AlgerianChequeFiller/
├── src/
│   └── AlgerianChequeFiller/
│       ├── App.xaml
│       ├── MainWindow.xaml
│       ├── Models/
│       │   ├── ChequeData.cs
│       │   ├── ChequeTemplate.cs
│       │   ├── FieldRect.cs
│       │   └── Language.cs
│       ├── Services/
│       │   ├── TemplateStore.cs
│       │   ├── NumberToWords/
│       │   │   ├── INumberToWordsConverter.cs
│       │   │   ├── FrenchConverter.cs
│       │   │   ├── ArabicConverter.cs
│       │   │   └── EnglishConverter.cs
│       │   ├── TextFitter.cs
│       │   ├── ChequeRenderer.cs
│       │   └── PrintService.cs
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   ├── ChequeFormViewModel.cs
│       │   ├── TemplateEditorViewModel.cs
│       │   └── CalibrationViewModel.cs
│       ├── Views/
│       │   ├── DisclaimerView.xaml
│       │   ├── ChequeFormView.xaml
│       │   ├── TemplateEditorView.xaml
│       │   ├── CalibrationWizardView.xaml
│       │   └── PreviewControl.xaml
│       └── Resources/
│           ├── Styles.xaml
│           └── default_template.json
├── tests/
│   └── AlgerianChequeFiller.Tests/
│       ├── NumberToWordsTests.cs
│       ├── CoordinateConversionTests.cs
│       ├── TextFitterTests.cs
│       └── TemplateTests.cs
└── README.md
```

---

## Data Models

### ChequeData.cs
```csharp
public class ChequeData
{
    public decimal Amount { get; set; }
    public string Beneficiary { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public Language Language { get; set; } = Language.French;
    
    public (long Dinars, int Centimes) GetAmountParts()
    {
        var dinars = (long)Math.Floor(Amount);
        var centimes = (int)((Amount - dinars) * 100);
        return (dinars, centimes);
    }
    
    public string FormattedAmount => Amount.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")) + " DA";
    public string FormattedDate => Date.ToString("dd/MM/yyyy");
}
```

### Language.cs
```csharp
public enum Language
{
    French,
    Arabic,
    English
}
```

### FieldRect.cs
```csharp
public record FieldRect(double X, double Y, double W, double H)
{
    // All values in millimeters
    
    public Rect ToWpfRect(double dpiX, double dpiY, double offsetX = 0, double offsetY = 0)
    {
        const double MmPerInch = 25.4;
        const double WpfDpi = 96.0;
        
        // Convert mm to WPF DIPs (device-independent pixels)
        double xDip = (X + offsetX) / MmPerInch * WpfDpi;
        double yDip = (Y + offsetY) / MmPerInch * WpfDpi;
        double wDip = W / MmPerInch * WpfDpi;
        double hDip = H / MmPerInch * WpfDpi;
        
        return new Rect(xDip, yDip, wDip, hDip);
    }
}
```

### ChequeTemplate.cs
```csharp
public class ChequeTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TemplateName { get; set; } = "Default";
    public SizeMm ChequeSizeMm { get; set; } = new(160, 80);
    public PointMm GlobalOffsetMm { get; set; } = new(0, 0);
    public Dictionary<FieldId, FieldRect> Fields { get; set; } = new();
    public FontSettings Fonts { get; set; } = new();
}

public record SizeMm(double Width, double Height);
public record PointMm(double X, double Y);

public enum FieldId
{
    AmountNumeric,
    AmountWordsL1,
    AmountWordsL2,
    Beneficiary,
    Place,
    Date
}

public class FontSettings
{
    public string LatinFamily { get; set; } = "Arial";
    public string ArabicFamily { get; set; } = "Traditional Arabic";
    public double MinSize { get; set; } = 8;
    public double MaxSize { get; set; } = 12;
}
```

---

## Default Template JSON

**File:** `Resources/default_template.json`

```json
{
  "id": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
  "templateName": "Algerian Cheque – Default",
  "chequeSizeMm": {
    "width": 160.0,
    "height": 80.0
  },
  "globalOffsetMm": {
    "x": 0.0,
    "y": 0.0
  },
  "fields": {
    "AmountNumeric": {
      "x": 140.0,
      "y": 8.0,
      "w": 32.0,
      "h": 8.0,
      "alignment": "Right"
    },
    "AmountWordsL1": {
      "x": 50.0,
      "y": 20.0,
      "w": 120.0,
      "h": 7.0,
      "alignment": "Left"
    },
    "Beneficiary": {
      "x": 38.0,
      "y": 30.0,
      "w": 130.0,
      "h": 8.0,
      "alignment": "Left"
    },
    "AmountWordsL2": {
      "x": 10.0,
      "y": 38.0,
      "w": 80.0,
      "h": 7.0,
      "alignment": "Left"
    },
    "Place": {
      "x": 100.0,
      "y": 50.0,
      "w": 35.0,
      "h": 7.0,
      "alignment": "Left"
    },
    "Date": {
      "x": 140.0,
      "y": 50.0,
      "w": 32.0,
      "h": 7.0,
      "alignment": "Left"
    }
  },
  "fonts": {
    "latinFamily": "Arial",
    "arabicFamily": "Traditional Arabic",
    "minSize": 8.0,
    "maxSize": 12.0
  }
}
```

---

## Coordinate System & Conversion

### Origin and Units
- **Origin**: Top-left corner of cheque
- **Template units**: Millimeters (mm)
- **WPF units**: Device-Independent Pixels (DIPs), where 1 DIP = 1/96 inch

### Conversion Formulas

```csharp
public static class CoordinateConverter
{
    private const double MmPerInch = 25.4;
    private const double WpfDpi = 96.0;
    
    /// <summary>
    /// Convert millimeters to WPF DIPs
    /// </summary>
    public static double MmToDip(double mm)
    {
        return mm / MmPerInch * WpfDpi;
    }
    
    /// <summary>
    /// Convert WPF DIPs to millimeters
    /// </summary>
    public static double DipToMm(double dip)
    {
        return dip / WpfDpi * MmPerInch;
    }
    
    /// <summary>
    /// Convert mm to printer pixels at given DPI
    /// </summary>
    public static double MmToPixels(double mm, double dpi)
    {
        return mm / MmPerInch * dpi;
    }
    
    /// <summary>
    /// Apply global offset and convert rect
    /// </summary>
    public static Rect ConvertFieldRect(FieldRect field, PointMm offset)
    {
        return new Rect(
            MmToDip(field.X + offset.X),
            MmToDip(field.Y + offset.Y),
            MmToDip(field.W),
            MmToDip(field.H)
        );
    }
}
```

### Printer DPI Handling

WPF's printing system automatically handles DPI scaling when using `DrawingVisual` and `VisualBrush`. The key is to:
1. Create content in DIPs
2. Set the correct page size on `PrintTicket`
3. Let the print system scale appropriately

```csharp
// Set page size in hundredths of an inch
printTicket.PageMediaSize = new PageMediaSize(
    chequeSizeMm.Width / 25.4 * 100,  // Width in 1/100 inch
    chequeSizeMm.Height / 25.4 * 100  // Height in 1/100 inch
);
```

---

## Number-to-Words Converters

### Interface
```csharp
public interface INumberToWordsConverter
{
    string Convert(decimal amount);
    bool IsRtl { get; }
}
```

### French Converter (FrenchConverter.cs)

**Rules:**
- "Un" becomes "Une" before feminine nouns (not used here)
- "Vingt" and "Cent" take "s" when multiplied and not followed by another number
- "Mille" is invariable (no "s")

```csharp
public class FrenchConverter : INumberToWordsConverter
{
    public bool IsRtl => false;
    
    private static readonly string[] Units = 
        { "", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf" };
    private static readonly string[] Tens = 
        { "", "dix", "vingt", "trente", "quarante", "cinquante", "soixante", "soixante", "quatre-vingt", "quatre-vingt" };
    private static readonly string[] Teens = 
        { "dix", "onze", "douze", "treize", "quatorze", "quinze", "seize", "dix-sept", "dix-huit", "dix-neuf" };
    
    public string Convert(decimal amount)
    {
        var (dinars, centimes) = SplitAmount(amount);
        
        var result = new StringBuilder();
        
        if (dinars == 0 && centimes == 0)
            return "zéro dinar";
            
        if (dinars > 0)
        {
            result.Append(ConvertNumber(dinars));
            result.Append(dinars == 1 ? " dinar" : " dinars");
        }
        
        if (centimes > 0)
        {
            if (dinars > 0) result.Append(" et ");
            result.Append(ConvertNumber(centimes));
            result.Append(centimes == 1 ? " centime" : " centimes");
        }
        
        return result.ToString();
    }
    
    // ... implementation details
}
```

### Arabic Converter (ArabicConverter.cs)

**Rules:**
- Numbers 1-2 use dual form (مثنى)
- Numbers 3-10 use plural (جمع)
- Numbers 11-99 use singular (مفرد)
- Proper RTL output

```csharp
public class ArabicConverter : INumberToWordsConverter
{
    public bool IsRtl => true;
    
    public string Convert(decimal amount)
    {
        var (dinars, centimes) = SplitAmount(amount);
        
        if (dinars == 0 && centimes == 0)
            return "صفر دينار";
        
        var result = new StringBuilder();
        
        if (dinars > 0)
        {
            result.Append(ConvertNumber(dinars));
            result.Append(" ");
            result.Append(GetDinarWord(dinars));
        }
        
        if (centimes > 0)
        {
            if (dinars > 0) result.Append(" و");
            result.Append(ConvertNumber(centimes));
            result.Append(" ");
            result.Append(GetCentimeWord(centimes));
        }
        
        return result.ToString();
    }
    
    private string GetDinarWord(long n) => n switch
    {
        1 => "دينار جزائري",
        2 => "ديناران جزائريان",
        >= 3 and <= 10 => "دنانير جزائرية",
        _ => "دينارًا جزائريًا"
    };
    
    // ... implementation details
}
```

### English Converter (EnglishConverter.cs)

```csharp
public class EnglishConverter : INumberToWordsConverter
{
    public bool IsRtl => false;
    
    public string Convert(decimal amount)
    {
        var (dinars, centimes) = SplitAmount(amount);
        
        if (dinars == 0 && centimes == 0)
            return "zero Algerian dinars";
        
        var result = new StringBuilder();
        
        if (dinars > 0)
        {
            result.Append(ConvertNumber(dinars));
            result.Append(dinars == 1 ? " Algerian dinar" : " Algerian dinars");
        }
        
        if (centimes > 0)
        {
            if (dinars > 0) result.Append(" and ");
            result.Append(ConvertNumber(centimes));
            result.Append(centimes == 1 ? " centime" : " centimes");
        }
        
        return result.ToString();
    }
}
```

### Test Cases

| Amount | French | English |
|--------|--------|---------|
| 0 | zéro dinar | zero Algerian dinars |
| 1 | un dinar | one Algerian dinar |
| 2.05 | deux dinars et cinq centimes | two Algerian dinars and five centimes |
| 10.10 | dix dinars et dix centimes | ten Algerian dinars and ten centimes |
| 11 | onze dinars | eleven Algerian dinars |
| 21 | vingt et un dinars | twenty-one Algerian dinars |
| 99.99 | quatre-vingt-dix-neuf dinars et quatre-vingt-dix-neuf centimes | ninety-nine Algerian dinars and ninety-nine centimes |
| 100 | cent dinars | one hundred Algerian dinars |
| 101 | cent un dinars | one hundred one Algerian dinars |
| 110 | cent dix dinars | one hundred ten Algerian dinars |
| 1000 | mille dinars | one thousand Algerian dinars |
| 1000000 | un million de dinars | one million Algerian dinars |

---

## Text Fitting Algorithm

### Overview
The amount-in-words must fit into two rectangles (L1 and L2). The algorithm tries to optimize readability while ensuring text fits.

### TextFitter.cs

```csharp
public class TextFitter
{
    public record FitResult(List<string> Lines, double FontSize, bool Success);
    
    public FitResult Fit(
        string text,
        Rect line1Rect,
        Rect line2Rect,
        string fontFamily,
        double minFontSize,
        double maxFontSize,
        bool isRtl)
    {
        // Step 1: Try single line at max font size in L2 (main line)
        if (TextFits(text, line2Rect, fontFamily, maxFontSize))
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
        var formattedText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily),
            fontSize,
            Brushes.Black,
            VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
        
        return formattedText.Width <= rect.Width && formattedText.Height <= rect.Height;
    }
    
    private List<string>? FindBestSplit(
        string[] words, Rect line1, Rect line2, string fontFamily, double fontSize)
    {
        // Try different split points, looking for balanced lines
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
}
```

### Definition of "Fits"
Text fits in a rectangle when:
1. `FormattedText.Width <= Rect.Width`
2. `FormattedText.Height <= Rect.Height`

Measurement is done using WPF's `FormattedText` class with the actual font and size.

---

## Printing System

### PrintService.cs

```csharp
public class PrintService
{
    public void Print(ChequeData data, ChequeTemplate template, bool testMode)
    {
        var printDialog = new PrintDialog();
        
        // Set custom page size
        var ticket = printDialog.PrintTicket;
        ticket.PageMediaSize = new PageMediaSize(
            PageMediaSizeName.Unknown,
            template.ChequeSizeMm.Width / 25.4 * 96,  // Convert to 1/96 inch
            template.ChequeSizeMm.Height / 25.4 * 96
        );
        
        // Disable scaling
        ticket.PageScalingFactor = 100;
        
        if (printDialog.ShowDialog() == true)
        {
            var visual = RenderCheque(data, template, testMode);
            printDialog.PrintVisual(visual, testMode ? "Calibration Test" : "Cheque Print");
        }
    }
    
    private DrawingVisual RenderCheque(ChequeData data, ChequeTemplate template, bool testMode)
    {
        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();
        
        var renderer = new ChequeRenderer(dc, template, testMode);
        renderer.Render(data);
        
        return visual;
    }
}
```

### ChequeRenderer.cs

```csharp
public class ChequeRenderer
{
    private readonly DrawingContext _dc;
    private readonly ChequeTemplate _template;
    private readonly bool _testMode;
    
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
        
        // Draw amount in words (fitted to 2 lines)
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
        foreach (var (fieldId, fieldRect) in _template.Fields)
        {
            var rect = CoordinateConverter.ConvertFieldRect(fieldRect, _template.GlobalOffsetMm);
            var color = GetFieldColor(fieldId);
            
            // Rectangle
            _dc.DrawRectangle(null, new Pen(new SolidColorBrush(color), 0.5), rect);
            
            // Crosshair at center
            var center = new Point(rect.X + rect.Width/2, rect.Y + rect.Height/2);
            _dc.DrawLine(new Pen(new SolidColorBrush(color), 0.3),
                new Point(center.X - 2, center.Y), new Point(center.X + 2, center.Y));
            _dc.DrawLine(new Pen(new SolidColorBrush(color), 0.3),
                new Point(center.X, center.Y - 2), new Point(center.X, center.Y + 2));
            
            // Label
            var label = new FormattedText(fieldId.ToString(), CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Arial"), 6, new SolidColorBrush(color), 1);
            _dc.DrawText(label, new Point(rect.X, rect.Y - 8));
        }
    }
}
```

### Print Modes

| Mode | Purpose | Output |
|------|---------|--------|
| **Test Print** | Calibration | Bounding boxes + labels + crosshairs on blank paper |
| **Real Print** | Production | Text only, positioned in fields |

---

## User Interface

### 1. Disclaimer Screen (First Launch)

**Purpose:** Legal acknowledgment before use

```xaml
<Window Title="Avertissement Légal">
    <StackPanel Margin="20">
        <TextBlock FontWeight="Bold" FontSize="16">
            ⚠️ AVERTISSEMENT IMPORTANT
        </TextBlock>
        
        <TextBlock TextWrapping="Wrap" Margin="0,10">
            Cette application est un outil de mise en forme pour l'impression 
            de chèques. En utilisant cette application, vous confirmez que:
            
            • Vous êtes autorisé(e) à émettre des chèques sur le compte concerné
            • Les informations saisies sont exactes et véridiques
            • Vous assumez l'entière responsabilité de l'utilisation de cette application
            
            Cette application n'est pas un système bancaire et ne valide pas 
            les informations saisies.
        </TextBlock>
        
        <CheckBox x:Name="AcceptCheckbox" Margin="0,10">
            J'ai lu et j'accepte les conditions ci-dessus
        </CheckBox>
        
        <Button Content="Continuer" IsEnabled="{Binding IsChecked, ElementName=AcceptCheckbox}"
                Click="OnAcceptClick" HorizontalAlignment="Right"/>
    </StackPanel>
</Window>
```

### 2. Cheque Form Screen (Main)

**Components:**
- Amount input (numeric, formatted as currency)
- Beneficiary text field
- Place text field
- Date picker
- Language selector (FR/AR/EN radio buttons)
- Live preview panel
- Print buttons

### 3. Template Editor Screen

**Components:**
- Canvas showing cheque outline (scaled)
- Draggable/resizable field rectangles
- Numeric inputs for X, Y, W, H (in mm)
- Global offset X/Y inputs
- Template name editor
- Save/Reset buttons

### 4. Calibration Wizard

**Steps:**
1. **Print Test Page** - Prints bounding boxes on blank paper
2. **Measure Offset** - User measures drift from expected positions
3. **Apply Correction** - Enter X/Y offset in mm
4. **Verify** - Print again to confirm alignment

---

## Testing

### Unit Tests

#### NumberToWordsTests.cs
```csharp
public class FrenchConverterTests
{
    private readonly FrenchConverter _converter = new();
    
    [Theory]
    [InlineData(0, "zéro dinar")]
    [InlineData(1, "un dinar")]
    [InlineData(2.05, "deux dinars et cinq centimes")]
    [InlineData(10.10, "dix dinars et dix centimes")]
    [InlineData(11, "onze dinars")]
    [InlineData(21, "vingt et un dinars")]
    [InlineData(99.99, "quatre-vingt-dix-neuf dinars et quatre-vingt-dix-neuf centimes")]
    [InlineData(100, "cent dinars")]
    [InlineData(101, "cent un dinars")]
    [InlineData(110, "cent dix dinars")]
    [InlineData(1000, "mille dinars")]
    [InlineData(1000000, "un million de dinars")]
    public void Convert_ReturnsCorrectFrench(decimal amount, string expected)
    {
        var result = _converter.Convert(amount);
        Assert.Equal(expected, result);
    }
}
```

#### CoordinateConversionTests.cs
```csharp
public class CoordinateConverterTests
{
    [Fact]
    public void MmToDip_ConvertsCorrectly()
    {
        // 25.4mm = 1 inch = 96 DIPs
        Assert.Equal(96, CoordinateConverter.MmToDip(25.4), 0.001);
    }
    
    [Fact]
    public void DipToMm_ConvertsCorrectly()
    {
        Assert.Equal(25.4, CoordinateConverter.DipToMm(96), 0.001);
    }
    
    [Fact]
    public void RoundTrip_PreservesValue()
    {
        var original = 42.5;
        var result = CoordinateConverter.DipToMm(CoordinateConverter.MmToDip(original));
        Assert.Equal(original, result, 3);
    }
}
```

### Manual Verification Checklist

- [ ] App launches without error
- [ ] Disclaimer appears on first launch
- [ ] Disclaimer acceptance is persisted
- [ ] Amount input formats correctly
- [ ] Amount-in-words updates in real-time
- [ ] All three languages work (FR/AR/EN)
- [ ] Arabic text displays RTL correctly
- [ ] Preview shows correct field positions
- [ ] Test print shows bounding boxes
- [ ] Global offset adjustments work
- [ ] Template changes are saved
- [ ] Print dialog appears with correct page size
- [ ] Printed output matches preview

---

## Legal Disclaimer

### Terms of Use

This software is provided as a document formatting tool for printing on pre-printed cheque stock. By using this software, you acknowledge and agree that:

1. **Authorization**: You are duly authorized to issue cheques on the bank account(s) for which you use this software.

2. **Accuracy**: You are solely responsible for the accuracy of all information entered, including amounts, beneficiary names, dates, and other details.

3. **No Banking Function**: This software does not perform any banking transactions, validate cheque details, or connect to any financial systems.

4. **Liability**: The developers and distributors of this software accept no liability for any financial loss, fraud, or legal issues arising from the use of this software.

5. **Compliance**: You are responsible for ensuring your use of this software complies with all applicable laws and banking regulations in Algeria.

6. **No Warranty**: This software is provided "as is" without warranty of any kind, express or implied.

### First-Launch Acknowledgment

Users must explicitly accept these terms on first launch before the application can be used. Acceptance is stored locally and checked on each application start.

---

## Non-Functional Requirements

| Requirement | Implementation |
|-------------|----------------|
| **Offline Operation** | No network access required |
| **Data Storage** | Templates stored in `%APPDATA%\AlgerianChequeFiller\` |
| **Error Handling** | User-friendly messages for common errors |
| **Logging** | Optional, via `Serilog` to `%APPDATA%\AlgerianChequeFiller\logs\` |
| **Localization** | French UI by default |

### Error Messages

| Scenario | Message |
|----------|---------|
| No printers found | "Aucune imprimante détectée. Veuillez installer une imprimante." |
| Invalid amount | "Montant invalide. Veuillez entrer un nombre positif." |
| Empty beneficiary | "Veuillez saisir le nom du bénéficiaire." |
| Template load failed | "Erreur de chargement du modèle. Utilisation du modèle par défaut." |

---

## Build & Release

### Build Commands
```powershell
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Run tests
dotnet test

# Publish self-contained
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
```

### Release Checklist
- [ ] All tests pass
- [ ] Version number updated
- [ ] README updated
- [ ] Default template verified
- [ ] Installer created (optional: WiX/Inno Setup)
- [ ] Code signed (optional)

---

## License

MIT License - See LICENSE file for details.

---

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Submit a pull request

---

*Last updated: December 2025*
