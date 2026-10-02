using System.Diagnostics;
using System.Windows;
using RhosTool.App.Services;
using RhosTool.Core.Services;

namespace RhosTool.App;

public partial class LoginWindow : Window
{
    private readonly AuthService _auth;

    public LoginWindow(AuthService auth)
    {
        _auth = auth;
        InitializeComponent();

        if (!_auth.IsConfigured)
        {
            StatusText.Text =
                "Config Supabase manquante. Éditez config.json (URL + anon key). Voir docs/AUTH-SUPABASE.md";
            StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
        }
        else
        {
            StatusText.Text = "Entrez vos identifiants Supabase.";
        }
    }

    private async void Login_Click(object sender, RoutedEventArgs e) =>
        await RunAuthAsync(register: false);

    private async void Register_Click(object sender, RoutedEventArgs e) =>
        await RunAuthAsync(register: true);

    private async Task RunAuthAsync(bool register)
    {
        SetBusy(true);
        try
        {
            var email = EmailBox.Text?.Trim() ?? "";
            var password = PasswordBox.Password ?? "";
            var username = UsernameBox.Text?.Trim();

            var result = register
                ? await _auth.RegisterAsync(email, password, username)
                : await _auth.LoginAsync(email, password);

            if (!result.Success || result.Session is null)
            {
                StatusText.Text = result.Message;
                StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
                return;
            }

            LocalAuthStore.SaveSession(result.Session);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
            StatusText.Foreground = System.Windows.Media.Brushes.OrangeRed;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OpenConfigFolder_Click(object sender, RoutedEventArgs e)
    {
        LocalAuthStore.LoadOrCreateConfig();
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = LocalAuthStore.AppDataDir,
            UseShellExecute = true
        });
        StatusText.Text = $"Config : {LocalAuthStore.ConfigPath}";
        StatusText.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
    }

    private void SetBusy(bool busy)
    {
        LoginButton.IsEnabled = !busy;
        RegisterButton.IsEnabled = !busy;
        EmailBox.IsEnabled = !busy;
        PasswordBox.IsEnabled = !busy;
        UsernameBox.IsEnabled = !busy;
        if (busy)
        {
            StatusText.Text = "Connexion en cours…";
            StatusText.Foreground = System.Windows.Media.Brushes.LightGray;
        }
    }
}
