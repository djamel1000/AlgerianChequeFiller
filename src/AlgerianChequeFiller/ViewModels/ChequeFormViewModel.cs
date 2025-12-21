using System.Windows.Media;
using AlgerianChequeFiller.Models;
using AlgerianChequeFiller.Services;
using AlgerianChequeFiller.Services.NumberToWords;

namespace AlgerianChequeFiller.ViewModels;

/// <summary>
/// ViewModel for the cheque form and preview.
/// </summary>
public class ChequeFormViewModel : ViewModelBase
{
    private readonly TemplateStore _templateStore;
    private readonly PrintService _printService;

    private decimal _amount;
    private string _beneficiary = string.Empty;
    private string _place = string.Empty;
    private DateTime _date = DateTime.Today;
    private Language _language = Language.French;
    private string _amountInWords = string.Empty;
    private DrawingImage? _previewImage;

    public ChequeFormViewModel()
    {
        _templateStore = new TemplateStore();
        _printService = new PrintService();
        Template = _templateStore.LoadDefaultTemplate();

        PrintCommand = new RelayCommand(ExecutePrint, CanPrint);
        TestPrintCommand = new RelayCommand(ExecuteTestPrint);

        UpdateAmountInWords();
        UpdatePreview();
    }

    public ChequeTemplate Template { get; private set; }

    public decimal Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, value))
            {
                UpdateAmountInWords();
                UpdatePreview();
            }
        }
    }

    public string AmountText
    {
        get => _amount.ToString("N2");
        set
        {
            if (decimal.TryParse(value, out var amount))
            {
                Amount = amount;
            }
        }
    }

    public string Beneficiary
    {
        get => _beneficiary;
        set
        {
            if (SetProperty(ref _beneficiary, value))
                UpdatePreview();
        }
    }

    public string Place
    {
        get => _place;
        set
        {
            if (SetProperty(ref _place, value))
                UpdatePreview();
        }
    }

    public DateTime Date
    {
        get => _date;
        set
        {
            if (SetProperty(ref _date, value))
                UpdatePreview();
        }
    }

    public Language Language
    {
        get => _language;
        set
        {
            if (SetProperty(ref _language, value))
            {
                UpdateAmountInWords();
                UpdatePreview();
            }
        }
    }

    public bool IsFrench
    {
        get => _language == Language.French;
        set { if (value) Language = Language.French; }
    }

    public bool IsArabic
    {
        get => _language == Language.Arabic;
        set { if (value) Language = Language.Arabic; }
    }

    public bool IsEnglish
    {
        get => _language == Language.English;
        set { if (value) Language = Language.English; }
    }

    public string AmountInWords
    {
        get => _amountInWords;
        private set => SetProperty(ref _amountInWords, value);
    }

    public DrawingImage? PreviewImage
    {
        get => _previewImage;
        private set => SetProperty(ref _previewImage, value);
    }

    public RelayCommand PrintCommand { get; }
    public RelayCommand TestPrintCommand { get; }

    private ChequeData CreateChequeData()
    {
        return new ChequeData
        {
            Amount = Amount,
            Beneficiary = Beneficiary,
            Place = Place,
            Date = Date,
            Language = Language
        };
    }

    private void UpdateAmountInWords()
    {
        var converter = ConverterFactory.Create(Language);
        AmountInWords = converter.Convert(Amount);
    }

    private void UpdatePreview()
    {
        var data = CreateChequeData();
        var visual = _printService.RenderCheque(data, Template, false);

        var drawingGroup = new DrawingGroup();
        using (var dc = drawingGroup.Open())
        {
            var bounds = new System.Windows.Rect(0, 0,
                CoordinateConverter.MmToDip(Template.ChequeSizeMm.Width),
                CoordinateConverter.MmToDip(Template.ChequeSizeMm.Height));
            dc.DrawRectangle(new VisualBrush(visual), null, bounds);
        }

        PreviewImage = new DrawingImage(drawingGroup);
    }

    private bool CanPrint(object? parameter)
    {
        return Amount > 0 && !string.IsNullOrWhiteSpace(Beneficiary);
    }

    private void ExecutePrint()
    {
        var data = CreateChequeData();
        _printService.Print(data, Template, false);
    }

    private void ExecuteTestPrint()
    {
        var data = CreateChequeData();
        _printService.Print(data, Template, true);
    }

    public void Refresh()
    {
        UpdateAmountInWords();
        UpdatePreview();
    }
}
