using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace DataFilter.UwpXaml.Controls;

public sealed class FilterPopupControl : UserControl
{
    public ColumnFilterViewModel? ViewModel { get; private set; }

    public FilterPopupControl()
    {
        var search = new TextBox { PlaceholderText = "Search..." };
        var add = new CheckBox { Content = "Add selection to filter" };
        var ok = new Button { Content = "OK" };
        var clear = new Button { Content = "Clear" };

        search.TextChanged += async (_, _) =>
        {
            if (ViewModel != null) await ViewModel.SearchCommand.ExecuteAsync(search.Text);
        };
        add.Checked += (_, _) => { if (ViewModel != null) ViewModel.AddToExistingFilter = true; };
        add.Unchecked += (_, _) => { if (ViewModel != null) ViewModel.AddToExistingFilter = false; };
        ok.Click += (_, _) => ViewModel?.ApplyCommand.Execute(null);
        clear.Click += (_, _) => ViewModel?.ClearCommand.Execute(null);

        Content = new StackPanel
        {
            Spacing = 8,
            Children = { search, add, new StackPanel { Orientation = Orientation.Horizontal, Children = { ok, clear } } }
        };
    }

    public void Bind(ColumnFilterViewModel vm) => ViewModel = vm;
}
