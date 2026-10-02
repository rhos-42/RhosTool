using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RhosTool.App.Converters;

public sealed class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var hex = value as string ?? "#444444";
        try
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
            brush.Freeze();
            return brush;
        }
        catch
        {
            return Brushes.DimGray;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class TagToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var tag = (value as string)?.ToLowerInvariant() ?? "";
        var hex = tag switch
        {
            "testpoint" => "#E53935",
            "preloader" => "#D81B60",
            "custom da" => "#8E24AA",
            "edl" => "#43A047",
            "odin" => "#FB8C00",
            "adb" => "#1E88E5",
            "fastboot" => "#00ACC1",
            "ios" => "#90A4AE",
            "recovery" => "#6D4C41",
            _ => "#616161"
        };
        var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
        brush.Freeze();
        return brush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
