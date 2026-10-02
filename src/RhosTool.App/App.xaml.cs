using System.Windows;
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

    private void ShowMain()
    {
        var main = new MainWindow
        {
            DataContext = new MainViewModel(Auth)
        };
        MainWindow = main;
        main.Show();
    }
}
