using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        WindowScreenFit.Apply(this, preferredWidth: 460, preferredHeight: 640, minWidth: 340, minHeight: 420);
        ShowPanel(login: true);

        var remembered = LocalAuthStore.LastUsername();
        if (!string.IsNullOrWhiteSpace(remembered))
        {
            LoginUserBox.Text = remembered;
            LoginPasswordBox.Focus();
        }

        if (!_auth.IsConfigured)
        {
            SetStatus("Config Supabase manquante. Éditez %AppData%\\RhosTool\\config.json", Brushes.OrangeRed);
        }
        else
        {
            ClearStatus();
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            return;
        }

        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ShowLogin_Click(object sender, RoutedEventArgs e) => ShowPanel(login: true);

    private void ShowActivate_Click(object sender, RoutedEventArgs e)
    {
        ActivateUserBox.Text = LoginUserBox.Text;
        ShowPanel(activate: true);
        ClearStatus();
    }

    private void ShowForgot_Click(object sender, RoutedEventArgs e)
    {
        var identity = LoginUserBox.Text?.Trim() ?? "";
        ForgotEmailBox.Text = LocalAuthStore.ResolveEmail(identity) ?? (identity.Contains('@') ? identity : "");
        ShowPanel(forgot: true);
        ClearStatus();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        SetBusy(true);
        try
        {
            var identity = LoginUserBox.Text?.Trim() ?? "";
            var password = LoginPasswordBox.Password ?? "";
            var email = LocalAuthStore.ResolveEmail(identity);

            if (string.IsNullOrWhiteSpace(email))
            {
                SetStatus(
                    "Nom d'utilisateur inconnu. Utilisez votre email, ou créez un compte.",
                    Brushes.OrangeRed);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                SetStatus("Mot de passe requis.", Brushes.OrangeRed);
                return;
            }

            var result = await _auth.LoginAsync(email, password);
            if (!result.Success || result.Session is null)
            {
                SetStatus(result.Message, Brushes.OrangeRed);
                return;
            }

            if (!string.IsNullOrWhiteSpace(identity) && !identity.Contains('@'))
            {
                LocalAuthStore.RememberIdentity(identity, email);
            }

            LocalAuthStore.SaveSession(result.Session);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, Brushes.OrangeRed);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void Activate_Click(object sender, RoutedEventArgs e)
    {
        SetBusy(true);
        try
        {
            var email = ActivateEmailBox.Text?.Trim() ?? "";
            var username = ActivateUserBox.Text?.Trim() ?? "";
            var password = ActivatePasswordBox.Password ?? "";

            if (string.IsNullOrWhiteSpace(username))
            {
                SetStatus("Nom d'utilisateur requis.", Brushes.OrangeRed);
                return;
            }

            var result = await _auth.RegisterAsync(email, password, username);
            if (!result.Success)
            {
                SetStatus(result.Message, Brushes.OrangeRed);
                return;
            }

            LocalAuthStore.RememberIdentity(username, email);

            if (result.Session is not null)
            {
                LocalAuthStore.SaveSession(result.Session);
                DialogResult = true;
                Close();
                return;
            }

            SetStatus(result.Message + " Vous pouvez vous connecter.", Brushes.LightGreen);
            LoginUserBox.Text = username;
            ShowPanel(login: true);
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, Brushes.OrangeRed);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void Forgot_Click(object sender, RoutedEventArgs e)
    {
        SetBusy(true);
        try
        {
            var email = ForgotEmailBox.Text?.Trim() ?? "";
            var result = await _auth.RecoverPasswordAsync(email);
            SetStatus(result.Message, result.Success ? Brushes.LightGreen : Brushes.OrangeRed);
            if (result.Success)
            {
                ShowPanel(login: true);
            }
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, Brushes.OrangeRed);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ShowPanel(bool login = false, bool activate = false, bool forgot = false)
    {
        LoginPanel.Visibility = login ? Visibility.Visible : Visibility.Collapsed;
        ActivatePanel.Visibility = activate ? Visibility.Visible : Visibility.Collapsed;
        ForgotPanel.Visibility = forgot ? Visibility.Visible : Visibility.Collapsed;

        SubtitleText.Text = activate ? "Créer un compte"
            : forgot ? "Mot de passe oublié"
            : "Connexion";
    }

    private void ClearStatus()
    {
        StatusText.Text = "";
        StatusBorder.Visibility = Visibility.Collapsed;
    }

    private void SetStatus(string text, Brush brush)
    {
        StatusText.Text = text;
        StatusText.Foreground = brush;
        StatusBorder.Visibility = string.IsNullOrWhiteSpace(text)
            ? Visibility.Collapsed
            : Visibility.Visible;

        if (brush == Brushes.LightGreen || brush == Brushes.LightGray)
            StatusBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x4A, 0x4A, 0x3A));
        else if (brush == Brushes.OrangeRed)
            StatusBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x8B, 0x3A, 0x3A));
        else
            StatusBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A));
    }

    private void SetBusy(bool busy)
    {
        LoginButton.IsEnabled = !busy;
        ActivateButton.IsEnabled = !busy;
        ForgotButton.IsEnabled = !busy;
        LoginUserBox.IsEnabled = !busy;
        LoginPasswordBox.IsEnabled = !busy;
        ActivateEmailBox.IsEnabled = !busy;
        ActivateUserBox.IsEnabled = !busy;
        ActivatePasswordBox.IsEnabled = !busy;
        ForgotEmailBox.IsEnabled = !busy;

        if (busy)
        {
            SetStatus("Patientez…", Brushes.LightGray);
        }
    }
}
