using System.Windows;

namespace AlgerianChequeFiller.Views;

/// <summary>
/// First-launch legal disclaimer window.
/// </summary>
public partial class DisclaimerWindow : Window
{
    public DisclaimerWindow()
    {
        InitializeComponent();
    }

    private void OnAcceptClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnRefuseClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
