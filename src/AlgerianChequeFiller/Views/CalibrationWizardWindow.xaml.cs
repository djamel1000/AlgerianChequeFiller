using System;
using System.Globalization;
using System.Windows;
using AlgerianChequeFiller.Models;
using AlgerianChequeFiller.Services;

namespace AlgerianChequeFiller.Views;

/// <summary>
/// Calibration wizard for adjusting printer offsets.
/// </summary>
public partial class CalibrationWizardWindow : Window
{
    private readonly ChequeTemplate _template;
    private readonly PrintService _printService;

    public double OffsetX { get; private set; }
    public double OffsetY { get; private set; }

    public CalibrationWizardWindow(ChequeTemplate template)
    {
        InitializeComponent();

        _template = template ?? throw new ArgumentNullException(nameof(template));
        _printService = new PrintService();

        // Initialize with current offsets
        OffsetXTextBox.Text = _template.GlobalOffsetMm.X.ToString("F1", CultureInfo.InvariantCulture);
        OffsetYTextBox.Text = _template.GlobalOffsetMm.Y.ToString("F1", CultureInfo.InvariantCulture);
    }

    private void OnPrintTestClick(object sender, RoutedEventArgs e)
    {
        // Apply current offset values for test print
        if (double.TryParse(OffsetXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            double.TryParse(OffsetYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            _template.GlobalOffsetMm = new PointMm(x, y);
        }

        var uiLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var testData = new ChequeData
        {
            Amount = 1234.56m,
            Beneficiary = "EXEMPLE DE BENEFICIAIRE",
            Place = "Alger",
            Date = DateTime.Today,
            Language = uiLang switch
            {
                "fr" => Language.French,
                "ar" => Language.Arabic,
                _ => Language.English,
            }
        };

        _printService.Print(testData, _template, testMode: true);
    }

    private void OnApplyClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(OffsetXTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            double.TryParse(OffsetYTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            OffsetX = x;
            OffsetY = y;
            DialogResult = true;
            Close();
            return;
        }

        MessageBox.Show(
            "Veuillez entrer des valeurs numériques valides.",
            "Erreur",
            MessageBoxButton.OK,
            MessageBoxImage.Warning
        );
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}