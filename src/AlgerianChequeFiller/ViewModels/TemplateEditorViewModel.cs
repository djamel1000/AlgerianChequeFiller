using System.ComponentModel;
using System.Runtime.CompilerServices;
using AlgerianChequeFiller.Models;
using AlgerianChequeFiller.Services;

namespace AlgerianChequeFiller.ViewModels;

/// <summary>
/// ViewModel for the template editor.
/// </summary>
public class TemplateEditorViewModel : ViewModelBase
{
    private readonly TemplateStore _templateStore;
    private ChequeTemplate _template;
    private string? _selectedFieldId;
    private double _selectedX;
    private double _selectedY;
    private double _selectedWidth;
    private double _selectedHeight;

    public TemplateEditorViewModel()
    {
        _templateStore = new TemplateStore();
        _template = _templateStore.LoadDefaultTemplate();

        SaveCommand = new RelayCommand(_ => Save());
        ResetCommand = new RelayCommand(_ => Reset());
    }

    public ChequeTemplate Template
    {
        get => _template;
        set => SetProperty(ref _template, value);
    }

    public string? SelectedFieldId
    {
        get => _selectedFieldId;
        set
        {
            if (SetProperty(ref _selectedFieldId, value))
            {
                LoadSelectedFieldProperties();
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }

    public bool HasSelection => !string.IsNullOrEmpty(_selectedFieldId);

    public double SelectedX
    {
        get => _selectedX;
        set
        {
            if (SetProperty(ref _selectedX, value))
                UpdateSelectedField();
        }
    }

    public double SelectedY
    {
        get => _selectedY;
        set
        {
            if (SetProperty(ref _selectedY, value))
                UpdateSelectedField();
        }
    }

    public double SelectedWidth
    {
        get => _selectedWidth;
        set
        {
            if (SetProperty(ref _selectedWidth, value))
                UpdateSelectedField();
        }
    }

    public double SelectedHeight
    {
        get => _selectedHeight;
        set
        {
            if (SetProperty(ref _selectedHeight, value))
                UpdateSelectedField();
        }
    }

    public double GlobalOffsetX
    {
        get => _template.GlobalOffsetMm.X;
        set
        {
            _template.GlobalOffsetMm = new PointMm(value, _template.GlobalOffsetMm.Y);
            OnPropertyChanged();
        }
    }

    public double GlobalOffsetY
    {
        get => _template.GlobalOffsetMm.Y;
        set
        {
            _template.GlobalOffsetMm = new PointMm(_template.GlobalOffsetMm.X, value);
            OnPropertyChanged();
        }
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand ResetCommand { get; }

    public event EventHandler? TemplateChanged;
    public event EventHandler? SaveRequested;

    private void LoadSelectedFieldProperties()
    {
        if (string.IsNullOrEmpty(_selectedFieldId) || 
            !_template.Fields.TryGetValue(_selectedFieldId, out var field))
        {
            _selectedX = 0;
            _selectedY = 0;
            _selectedWidth = 0;
            _selectedHeight = 0;
        }
        else
        {
            _selectedX = field.X;
            _selectedY = field.Y;
            _selectedWidth = field.W;
            _selectedHeight = field.H;
        }

        OnPropertyChanged(nameof(SelectedX));
        OnPropertyChanged(nameof(SelectedY));
        OnPropertyChanged(nameof(SelectedWidth));
        OnPropertyChanged(nameof(SelectedHeight));
    }

    private void UpdateSelectedField()
    {
        if (string.IsNullOrEmpty(_selectedFieldId))
            return;

        var field = new FieldRect(_selectedX, _selectedY, _selectedWidth, _selectedHeight);
        _template.Fields[_selectedFieldId] = field;
        TemplateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UpdateFieldPosition(string fieldId, double xMm, double yMm)
    {
        if (_template.Fields.TryGetValue(fieldId, out var field))
        {
            _template.Fields[fieldId] = new FieldRect(xMm, yMm, field.W, field.H);
            
            if (fieldId == _selectedFieldId)
            {
                _selectedX = xMm;
                _selectedY = yMm;
                OnPropertyChanged(nameof(SelectedX));
                OnPropertyChanged(nameof(SelectedY));
            }
            
            TemplateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UpdateFieldSize(string fieldId, double widthMm, double heightMm)
    {
        if (_template.Fields.TryGetValue(fieldId, out var field))
        {
            _template.Fields[fieldId] = new FieldRect(field.X, field.Y, widthMm, heightMm);
            
            if (fieldId == _selectedFieldId)
            {
                _selectedWidth = widthMm;
                _selectedHeight = heightMm;
                OnPropertyChanged(nameof(SelectedWidth));
                OnPropertyChanged(nameof(SelectedHeight));
            }
            
            TemplateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Save()
    {
        SaveRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SaveWithName(string templateName)
    {
        _templateStore.SaveTemplate(_template, templateName);
    }

    private void Reset()
    {
        _template = _templateStore.LoadDefaultTemplate();
        OnPropertyChanged(nameof(Template));
        OnPropertyChanged(nameof(GlobalOffsetX));
        OnPropertyChanged(nameof(GlobalOffsetY));
        SelectedFieldId = null;
        TemplateChanged?.Invoke(this, EventArgs.Empty);
    }
}
