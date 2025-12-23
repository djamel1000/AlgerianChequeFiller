using System.Windows;
using AlgerianChequeFiller.Views;

namespace AlgerianChequeFiller;

/// <summary>
/// Main application window.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnOpenTemplateEditor(object sender, RoutedEventArgs e)
    {
        var editor = new TemplateEditorWindow();
        editor.Owner = this;
        editor.ShowDialog();
    }
}
