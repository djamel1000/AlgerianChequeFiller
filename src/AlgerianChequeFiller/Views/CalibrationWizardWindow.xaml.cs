using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using AlgerianChequeFiller.Models;
using AlgerianChequeFiller.Services;

namespace AlgerianChequeFiller.Views
{
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

            // Set the language for the window based on current culture
            SetWindowLanguage();
        }

        /// <summary>
        /// Sets the appropriate language for the window content based on current UI culture.
        /// </summary>
        private void SetWindowLanguage()
        {
            var cultureName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            
            try
            {
                this.Language = cultureName switch
                {
                    "fr" => XmlLanguage.GetLanguage("fr-FR"),
                    "ar" => XmlLanguage.GetLanguage("ar-DZ"),
                    "en" => XmlLanguage.GetLanguage("en-US"),
                    _ => XmlLanguage.GetLanguage("fr-FR") // Default to French for Algeria
                };
            }
            catch (ArgumentException)
            {
                // If the language is not supported, fall back to French
                this.Language = XmlLanguage.GetLanguage("fr-FR");
            }
        }

        /// <summary>
        /// Handles the test print button click event.
        /// </summary>
        private void OnPrintTestClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseOffsets(out var offsetX, out var offsetY))
            {
                ShowValidationError("Veuillez entrer des valeurs numériques valides pour les décalages.");
                return;
            }

            // Apply current offset values for test print
            _template.GlobalOffsetMm = new PointMm(offsetX, offsetY);

            var testData = CreateTestChequeData();

            try
            {
                _printService.Print(testData, _template, testMode: true);
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de l'impression test: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the apply button click event.
        /// </summary>
        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseOffsets(out var offsetX, out var offsetY))
            {
                ShowValidationError("Veuillez entrer des valeurs numériques valides.");
                return;
            }

            OffsetX = offsetX;
            OffsetY = offsetY;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handles the cancel button click event.
        /// </summary>
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Attempts to parse the offset values from the textboxes.
        /// </summary>
        /// <param name="offsetX">The parsed X offset value.</param>
        /// <param name="offsetY">The parsed Y offset value.</param>
        /// <returns>True if both offsets were successfully parsed; otherwise, false.</returns>
        private bool TryParseOffsets(out double offsetX, out double offsetY)
        {
            offsetX = 0;
            offsetY = 0;

            var xText = OffsetXTextBox.Text?.Trim() ?? string.Empty;
            var yText = OffsetYTextBox.Text?.Trim() ?? string.Empty;

            return double.TryParse(xText, NumberStyles.Float, CultureInfo.InvariantCulture, out offsetX) &&
                   double.TryParse(yText, NumberStyles.Float, CultureInfo.InvariantCulture, out offsetY);
        }

        /// <summary>
        /// Creates test cheque data based on current UI culture.
        /// </summary>
        /// <returns>A ChequeData object with test values.</returns>
        private ChequeData CreateTestChequeData()
        {
            var uiLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var language = uiLang switch
            {
                "fr" => Language.French,
                "ar" => Language.Arabic,
                "en" => Language.English,
                _ => Language.French // Default to French
            };

            return new ChequeData
            {
                Amount = 1234.56m,
                Beneficiary = language switch
                {
                    Language.French => "EXEMPLE DE BÉNÉFICIAIRE",
                    Language.Arabic => "مثال على المستفيد",
                    Language.English => "SAMPLE BENEFICIARY",
                    _ => "EXEMPLE DE BÉNÉFICIAIRE"
                },
                Place = language switch
                {
                    Language.French => "Alger",
                    Language.Arabic => "الجزائر",
                    Language.English => "Algiers",
                    _ => "Alger"
                },
                Date = DateTime.Today,
                Language = language
            };
        }

        /// <summary>
        /// Shows a validation error message to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private void ShowValidationError(string message)
        {
            MessageBox.Show(
                message,
                "Erreur de validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

        /// <summary>
        /// Shows a general error message to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}