using System.Globalization;
using System.Windows;
using DataFilter.Localization;

namespace DataFilter.Wpf.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var options = LocalizationManager.GetAvailableCultures()
            .Select(c => new LanguageOption(c))
            .ToList();

        LanguageCombo.ItemsSource = options;
        LanguageCombo.DisplayMemberPath = nameof(LanguageOption.Label);
        LanguageCombo.SelectedValuePath = nameof(LanguageOption.Culture);
        LanguageCombo.SelectedValue = LocalizationManager.Instance.Culture;
        LanguageCombo.SelectionChanged += (_, __) =>
        {
            if (LanguageCombo.SelectedValue is CultureInfo culture)
                LocalizationManager.Instance.SetCulture(culture);
        };
    }

    private sealed class LanguageOption
    {
        public LanguageOption(CultureInfo culture)
        {
            Culture = culture;
            Label = culture == CultureInfo.InvariantCulture ? "Default" : culture.NativeName;
        }

        public CultureInfo Culture { get; }
        public string Label { get; }
    }
}
