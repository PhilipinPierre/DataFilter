using DataFilter.PlatformShared.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace DataFilter.WinUI3.Controls;

public sealed class FilterPopupControl : UserControl
{
    public ColumnFilterViewModel? ViewModel { get; private set; }

    public FilterPopupControl()
    {
        var search = new TextBox { PlaceholderText = "Search...", Margin = new Thickness(0, 0, 0, 8) };
        var ok = new Button { Content = "Apply", Margin = new Thickness(0, 0, 8, 0) };
        var clear = new Button { Content = "Clear" };

        search.TextChanged += async (_, _) =>
        {
            if (ViewModel != null) await ViewModel.SearchCommand.ExecuteAsync(search.Text);
        };
        ok.Click += (_, _) => ViewModel?.ApplyCommand.Execute(null);
        clear.Click += (_, _) => ViewModel?.ClearCommand.Execute(null);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(ok);
        buttons.Children.Add(clear);

        var root = new StackPanel { Padding = new Thickness(10) };
        root.Children.Add(search);
        root.Children.Add(buttons);

        Content = root;
    }

    public void Bind(ColumnFilterViewModel vm) => ViewModel = vm;
}
