using System.Windows.Controls;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// Interaction logic for FilterPopup.xaml
/// </summary>
public partial class FilterPopup : UserControl
{
    public event EventHandler? CancelRequested;

    public FilterPopup()
    {
        InitializeComponent();
    }

    private void OnCancelClick(object sender, System.Windows.RoutedEventArgs e)
        => CancelRequested?.Invoke(this, EventArgs.Empty);

    private void OnResizeThumbDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        double newWidth = ActualWidth + e.HorizontalChange;
        double newHeight = ActualHeight + e.VerticalChange;

        // Ensure we don't resize smaller than some reasonable minimums
        if (newWidth >= 150) Width = newWidth;
        if (newHeight >= 100) Height = newHeight;
    }
}
