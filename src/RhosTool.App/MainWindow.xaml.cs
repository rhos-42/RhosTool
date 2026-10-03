using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RhosTool.App.Services;
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
        WindowScreenFit.Apply(this, preferredWidth: 1280, preferredHeight: 820, minWidth: 720, minHeight: 480);
        Loaded += OnLoaded;
        SizeChanged += (_, _) => UpdateBrandStripHeight();
        StateChanged += (_, _) =>
        {
            UpdateMaxRestoreGlyph();
            Dispatcher.BeginInvoke(UpdateBrandStripHeight);
        };
        UpdateMaxRestoreGlyph();
        UpdateBrandStripHeight();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateBrandStripHeight();
        if (DataContext is MainViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }

    private void UpdateBrandStripHeight()
    {
        if (BrandRowDef is null || ActualWidth <= 0)
        {
            return;
        }

        BrandRowDef.Height = new GridLength(WindowScreenFit.BrandStripHeight(ActualWidth));
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

        MaxRestoreGlyph.Data = WindowState == WindowState.Maximized
            ? MaxRestoredGeometry
            : MaxNormalGeometry;
    }
}
