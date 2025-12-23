using System.Windows;

namespace AlgerianChequeFiller.Views;

public partial class SaveTemplateDialog : Window
{
    public string TemplateName { get; private set; } = string.Empty;

    public SaveTemplateDialog(string currentName = "")
    {
        InitializeComponent();
        TemplateNameBox.Text = currentName;
        TemplateNameBox.SelectAll();
        TemplateNameBox.Focus();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var name = TemplateNameBox.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("Veuillez entrer un nom pour le modèle.", 
                "Nom requis", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        TemplateName = name;
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
