using System.Windows;
using System.Windows.Controls;
using AlgerianChequeFiller.Services;
using AlgerianChequeFiller.ViewModels;
using AlgerianChequeFiller.Views;

namespace AlgerianChequeFiller;

/// <summary>
/// Main application window.
/// </summary>
public partial class MainWindow : Window
{
    private readonly TemplateStore _templateStore;
    private ChequeFormViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _templateStore = new TemplateStore();
        
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = DataContext as ChequeFormViewModel;
        RefreshTemplateList();
    }

    private void RefreshTemplateList()
    {
        TemplateSelector.Items.Clear();
        
        // Add default template
        TemplateSelector.Items.Add("Default (Built-in)");
        
        // Add saved templates
        var savedTemplates = _templateStore.GetSavedTemplateNames();
        foreach (var name in savedTemplates)
        {
            TemplateSelector.Items.Add(name);
        }
        
        TemplateSelector.SelectedIndex = 0;
    }

    private void OnTemplateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel == null || TemplateSelector.SelectedItem == null)
            return;

        var selectedName = TemplateSelector.SelectedItem.ToString();
        
        if (selectedName == "Default (Built-in)")
        {
            _viewModel.LoadTemplate(_templateStore.LoadDefaultTemplate());
        }
        else if (!string.IsNullOrEmpty(selectedName))
        {
            var template = _templateStore.LoadTemplate(selectedName);
            if (template != null)
            {
                _viewModel.LoadTemplate(template);
            }
        }
    }

    private void OnOpenTemplateEditor(object sender, RoutedEventArgs e)
    {
        var editor = new TemplateEditorWindow();
        editor.Owner = this;
        editor.ShowDialog();
        
        // Refresh template list after editor closes
        RefreshTemplateList();
    }
}
