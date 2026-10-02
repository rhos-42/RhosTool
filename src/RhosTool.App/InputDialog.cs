using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RhosTool.App;

internal static class InputDialog
{
    public static string? Prompt(Window owner, string title, string message, string defaultValue = "")
    {
        var window = new Window
        {
            Title = title,
            Width = 460,
            Height = 170,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.WhiteSmoke,
            ShowInTaskbar = false
        };

        var root = new DockPanel { Margin = new Thickness(14) };
        var label = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
        DockPanel.SetDock(label, Dock.Top);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 12, 0, 0)
        };
        DockPanel.SetDock(buttons, Dock.Bottom);

        var box = new TextBox
        {
            Text = defaultValue,
            Padding = new Thickness(6, 4, 6, 4)
        };

        var ok = new Button { Content = "OK", Width = 88, IsDefault = true, Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = "Annuler", Width = 88, IsCancel = true };
        string? result = null;

        ok.Click += (_, _) =>
        {
            result = box.Text?.Trim();
            window.DialogResult = true;
            window.Close();
        };
        cancel.Click += (_, _) =>
        {
            window.DialogResult = false;
            window.Close();
        };

        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        root.Children.Add(label);
        root.Children.Add(buttons);
        root.Children.Add(box);
        window.Content = root;
        window.Loaded += (_, _) =>
        {
            box.Focus();
            box.SelectAll();
            Keyboard.Focus(box);
        };

        return window.ShowDialog() == true ? result : null;
    }
}
