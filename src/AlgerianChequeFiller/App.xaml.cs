using System.Windows;
using AlgerianChequeFiller.Services;
using AlgerianChequeFiller.Views;

namespace AlgerianChequeFiller;

/// <summary>
/// Application startup logic.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var templateStore = new TemplateStore();

        // Show disclaimer on first launch
        if (!templateStore.IsDisclaimerAccepted())
        {
            var disclaimer = new DisclaimerWindow();
            var result = disclaimer.ShowDialog();

            if (result != true)
            {
                Shutdown();
                return;
            }

            templateStore.AcceptDisclaimer();
        }
    }
}
