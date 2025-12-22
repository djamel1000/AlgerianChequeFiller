using System.Windows;
using AlgerianChequeFiller.Models;
using AlgerianChequeFiller.Services;
using System.Globalization;
using System.Windows.Markup;
using AlgerianChequeFiller.Models;
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
        _template = template;
        _printService = new PrintService();

        // Initialize with current offsets
        OffsetXTextBox.Text = template.GlobalOffsetMm.X.ToString("F1");
        OffsetYTextBox.Text = template.GlobalOffsetMm.Y.ToString("F1");
    }

    private void OnPrintTestClick(object sender, RoutedEventArgs e)
    {
        // Apply current offset values for test print
        if (double.TryParse(OffsetXTextBox.Text, out var x) &&
            double.TryParse(OffsetYTextBox.Text, out var y))
        {
            _template.GlobalOffsetMm = new PointMm(x, y);
        }

        var testData = new ChequeData
        {
            Amount = 1234.56m,
            Beneficiary = "EXEMPLE DE BENEFICIAIRE",
            Place = "Alger",
            Date = DateTime.Today,
            Language = Language.French;
        };

        _printService.Print(testData, _template, testMode: true);
    }

    private void OnApplyClick(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(OffsetXTextBox.Text, out var x) &&
            double.TryParse(OffsetYTextBox.Text, out var y))
        {
            OffsetX = x;
            OffsetY = y;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Veuillez entrer des valeurs numériques valides.",
                "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
