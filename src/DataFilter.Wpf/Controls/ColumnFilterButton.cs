using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// A control that represents the filter button in a column header.
/// </summary>
public class ColumnFilterButton : Button
{
    static ColumnFilterButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnFilterButton), new FrameworkPropertyMetadata(typeof(ColumnFilterButton)));
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ColumnFilterButton), new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets a value indicating whether the filter is currently active (has selected criteria).
    /// </summary>
    public bool IsActive
    {
        get { return (bool)GetValue(IsActiveProperty); }
        set { SetValue(IsActiveProperty, value); }
    }

    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(ColumnFilterButton), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the data template used to render the icon.
    /// </summary>
    public DataTemplate IconTemplate
    {
        get { return (DataTemplate)GetValue(IconTemplateProperty); }
        set { SetValue(IconTemplateProperty, value); }
    }

    public static readonly DependencyProperty ActiveBrushProperty =
        DependencyProperty.Register(nameof(ActiveBrush), typeof(Brush), typeof(ColumnFilterButton), new PropertyMetadata(Brushes.DodgerBlue));

    /// <summary>
    /// Gets or sets the brush used when the filter is active.
    /// </summary>
    public Brush ActiveBrush
    {
        get { return (Brush)GetValue(ActiveBrushProperty); }
        set { SetValue(ActiveBrushProperty, value); }
    }

    public static readonly DependencyProperty InactiveBrushProperty =
        DependencyProperty.Register(nameof(InactiveBrush), typeof(Brush), typeof(ColumnFilterButton), new PropertyMetadata(Brushes.Gray));

    /// <summary>
    /// Gets or sets the brush used when the filter is inactive.
    /// </summary>
    public Brush InactiveBrush
    {
        get { return (Brush)GetValue(InactiveBrushProperty); }
        set { SetValue(InactiveBrushProperty, value); }
    }
}
