using System.Windows;
using System.Windows.Threading;
using RhosTool.App.Services;
using RhosTool.App.ViewModels;
using RhosTool.Core.Services;

namespace RhosTool.App;

public partial class App : Application
{
    public static AuthService Auth { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Empêche la fermeture auto quand LoginWindow se ferme (sinon plus de MainWindow)
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            MessageBox.Show(ex?.ToString() ?? "Erreur fatale", "RhosTool", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        try
        {
            var config = LocalAuthStore.LoadOrCreateConfig();
            Auth = new AuthService(config);

            var restored = LocalAuthStore.LoadSession();
            if (restored is not null && Auth.IsConfigured)
            {
                var restore = await Auth.RestoreSessionAsync(restored);
                if (restore.Success && Auth.CurrentSession is not null)
                {
                    LocalAuthStore.SaveSession(Auth.CurrentSession);
                    ShowMain();
                    return;
                }

                LocalAuthStore.ClearSession();
            }

            var login = new LoginWindow(Auth);
            var ok = login.ShowDialog() == true && Auth.IsAuthenticated;
            if (!ok)
            {
                Shutdown();
                return;
            }

            ShowMain();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Démarrage impossible :\n" + ex.Message,
                "RhosTool",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void ShowMain()
    {
        var main = new MainWindow
        {
            DataContext = new MainViewModel(Auth)
        };
        MainWindow = main;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        main.Show();
        main.Activate();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            "Erreur :\n" + e.Exception.Message,
            "RhosTool",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
