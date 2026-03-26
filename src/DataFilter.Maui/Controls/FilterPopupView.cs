using DataFilter.PlatformShared.ViewModels;

namespace DataFilter.Maui.Controls;

public sealed class FilterPopupView : ContentView
{
    public ColumnFilterViewModel? ViewModel { get; private set; }

    public FilterPopupView()
    {
        var search = new Entry { Placeholder = "Search..." };
        var addToFilter = new CheckBox();
        var addLabel = new Label { Text = "Add selection to filter", VerticalTextAlignment = TextAlignment.Center };
        var ok = new Button { Text = "OK" };
        var clear = new Button { Text = "Clear" };

        search.TextChanged += async (_, e) =>
        {
            if (ViewModel != null) await ViewModel.SearchCommand.ExecuteAsync(e.NewTextValue ?? string.Empty);
        };
        addToFilter.CheckedChanged += (_, e) =>
        {
            if (ViewModel != null) ViewModel.AddToExistingFilter = e.Value;
        };
        ok.Clicked += (_, _) => ViewModel?.ApplyCommand.Execute(null);
        clear.Clicked += (_, _) => ViewModel?.ClearCommand.Execute(null);

        Content = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                search,
                new HorizontalStackLayout { Children = { addToFilter, addLabel } },
                new HorizontalStackLayout { Children = { ok, clear } }
            }
        };
    }

    public void Bind(ColumnFilterViewModel vm) => ViewModel = vm;
}
