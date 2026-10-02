using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RhosTool.App.ViewModels;

namespace RhosTool.App;

public partial class MainWindow : Window
{
    private static readonly Geometry MaxNormalGeometry =
        Geometry.Parse("M 0,0 H 10 V 10 H 0 Z");

    private static readonly Geometry MaxRestoredGeometry =
        Geometry.Parse("M 2,0 H 10 V 8 H 2 Z M 0,2 H 8 V 10 H 0 Z");

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        StateChanged += (_, _) => UpdateMaxRestoreGlyph();
        UpdateMaxRestoreGlyph();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }

    private void Brands_PreviewMouseWheel(object sender, MouseWheelEventArgs e) =>
        e.Handled = true;

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void MaxRestore_Click(object sender, RoutedEventArgs e) =>
        ToggleMaximize();

    private void Close_Click(object sender, RoutedEventArgs e) =>
        Close();

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void UpdateMaxRestoreGlyph()
    {
        if (MaxRestoreGlyph is null)
        {
            return;
        }

        // Comme UnlockTool : carré simple en mode normal, doubles carrés si maximisé
        MaxRestoreGlyph.Data = WindowState == WindowState.Maximized
            ? MaxRestoredGeometry
            : MaxNormalGeometry;
    }
}
