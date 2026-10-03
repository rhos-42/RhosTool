using System.Windows;

namespace RhosTool.App.Services;

internal static class WindowScreenFit
{
    /// <summary>
    /// Dimensionne et centre une fenêtre dans la zone de travail (tous écrans / DPI).
    /// </summary>
    public static void Apply(
        Window window,
        double preferredWidth,
        double preferredHeight,
        double minWidth = 360,
        double minHeight = 320,
        double maxWorkRatio = 0.94,
        bool centerOnWorkArea = true)
    {
        var wa = SystemParameters.WorkArea;
        var maxW = Math.Max(320, wa.Width * maxWorkRatio);
        var maxH = Math.Max(280, wa.Height * maxWorkRatio);

        window.MinWidth = Math.Min(minWidth, maxW);
        window.MinHeight = Math.Min(minHeight, maxH);
        window.MaxWidth = wa.Width;
        window.MaxHeight = wa.Height;

        var w = Math.Clamp(preferredWidth, window.MinWidth, maxW);
        var h = Math.Clamp(preferredHeight, window.MinHeight, maxH);

        window.Width = w;
        window.Height = h;

        if (!centerOnWorkArea)
        {
            return;
        }

        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = wa.Left + (wa.Width - w) / 2;
        window.Top = wa.Top + (wa.Height - h) / 2;
    }

    public static double BrandStripHeight(double windowWidth)
    {
        // UnlockTool : tuiles plus hautes (≈ ratio 0.45), 3 rangées + gutters
        var tile = windowWidth / 10.0;
        var row = Math.Clamp(tile * 0.45, 38, 58);
        return Math.Clamp(row * 3 + 8, 120, 182);
    }
}
