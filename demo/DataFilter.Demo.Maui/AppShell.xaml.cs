using DataFilter.Maui.Demo.ViewModels;
using DataFilter.Localization;
using System.Globalization;

namespace DataFilter.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

        DirectionPicker.ItemsSource = new[] { "LTR", "RTL" };
        DirectionPicker.SelectedIndex = FlowDirection == FlowDirection.RightToLeft ? 1 : 0;
        DirectionPicker.SelectedIndexChanged += (_, __) =>
        {
            FlowDirection = DirectionPicker.SelectedIndex == 1 ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        };

            var options = LocalizationManager.GetAvailableCultures()
                .Select(c => new LanguageOption(c))
                .ToList();

            LanguagePicker.ItemsSource = options;
            LanguagePicker.ItemDisplayBinding = new Binding(nameof(LanguageOption.Label));
            LanguagePicker.SelectedItem = options.FirstOrDefault(o => Equals(o.Culture, LocalizationManager.Instance.Culture));
            LanguagePicker.SelectedIndexChanged += (_, __) =>
            {
                if (LanguagePicker.SelectedItem is LanguageOption opt)
                    LocalizationManager.Instance.SetCulture(opt.Culture);
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
}
