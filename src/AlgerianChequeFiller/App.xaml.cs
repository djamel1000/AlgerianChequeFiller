using System;
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

        // Add global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
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

            // Create and show main window
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erreur au démarrage de l'application:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Erreur de Démarrage",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show(
                $"Erreur non gérée:\n\n{ex.Message}",
                "Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Erreur:\n\n{e.Exception.Message}",
            "Erreur",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
