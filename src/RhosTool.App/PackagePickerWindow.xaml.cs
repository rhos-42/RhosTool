using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RhosTool.App.Services;

namespace RhosTool.App;

public partial class PackagePickerWindow : Window
{
    private readonly IReadOnlyList<string> _all;
    private readonly IReadOnlyList<string> _thirdParty;
    private readonly string _defaultValue;

    public string? SelectedPackage { get; private set; }
    public bool UseManualEntry { get; private set; }

    public PackagePickerWindow(
        Window owner,
        string title,
        IReadOnlyList<string> allPackages,
        IReadOnlyList<string>? thirdPartyPackages = null,
        string defaultValue = "",
        bool preferThirdParty = true)
    {
        InitializeComponent();
        Owner = owner;
        WindowScreenFit.Apply(
            this,
            preferredWidth: 520,
            preferredHeight: 560,
            minWidth: 320,
            minHeight: 360,
            centerOnWorkArea: false);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        TitleText.Text = title;
        _all = allPackages;
        _thirdParty = thirdPartyPackages ?? allPackages;
        _defaultValue = defaultValue;
        ThirdPartyBox.IsChecked = preferThirdParty;
        FilterBox.Text = "";
        ApplyFilter();

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            var match = PackageList.Items.Cast<string>()
                .FirstOrDefault(p => p.Equals(defaultValue, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                PackageList.SelectedItem = match;
                PackageList.ScrollIntoView(match);
            }
        }

        Loaded += (_, _) =>
        {
            FilterBox.Focus();
            Keyboard.Focus(FilterBox);
        };
    }

    private IReadOnlyList<string> CurrentSource =>
        ThirdPartyBox.IsChecked == true ? _thirdParty : _all;

    private void ApplyFilter()
    {
        var filter = FilterBox.Text?.Trim() ?? "";
        var source = CurrentSource;
        IEnumerable<string> q = source;
        if (!string.IsNullOrEmpty(filter))
        {
            q = source.Where(p => p.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        var list = q.ToList();
        PackageList.ItemsSource = list;
        CountText.Text = $"{list.Count} / {source.Count} package(s)";
    }

    private void FilterBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void ThirdPartyBox_Changed(object sender, RoutedEventArgs e) => ApplyFilter();

    private void PackageList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (PackageList.SelectedItem is string)
        {
            AcceptSelection();
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e) => AcceptSelection();

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Manual_Click(object sender, RoutedEventArgs e)
    {
        UseManualEntry = true;
        SelectedPackage = _defaultValue;
        DialogResult = true;
        Close();
    }

    private void AcceptSelection()
    {
        if (PackageList.SelectedItem is not string pkg || string.IsNullOrWhiteSpace(pkg))
        {
            MessageBox.Show(
                this,
                "Sélectionnez un package dans la liste.",
                "RhosTool",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        SelectedPackage = pkg;
        DialogResult = true;
        Close();
    }
}
