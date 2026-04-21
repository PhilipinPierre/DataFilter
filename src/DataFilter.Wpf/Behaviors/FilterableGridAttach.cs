using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DataFilter.Wpf.Behaviors;

/// <summary>
/// Attachable API to enable DataFilter column filtering on existing WPF grids without replacing controls.
/// </summary>
public static class FilterableGridAttach
{
    #region Attach attached property

    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached(
            "Attach",
            typeof(bool),
            typeof(FilterableGridAttach),
            new PropertyMetadata(false, OnAttachChanged));

    public static bool GetAttach(DependencyObject obj) => (bool)obj.GetValue(AttachProperty);
    public static void SetAttach(DependencyObject obj, bool value) => obj.SetValue(AttachProperty, value);

    private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool b || !b)
            return;

        switch (d)
        {
            case DataGrid dg:
                EnsureDataGridHeaderStyle(dg);
                break;
            case GridView gv:
                EnsureGridViewHeaderStyle(gv);
                break;
        }
    }

    private static void EnsureDataGridHeaderStyle(DataGrid dg)
    {
        var baseStyle = dg.ColumnHeaderStyle;
        if (baseStyle != null && baseStyle.TargetType != typeof(DataGridColumnHeader))
        {
            // Defensive: if user set a style for a different target type, don't override.
            return;
        }

        var style = new Style(typeof(DataGridColumnHeader), baseStyle);
        style.Setters.Add(new Setter(FilterableColumnHeaderBehavior.IsFilterableProperty, true));
        dg.ColumnHeaderStyle = style;
    }

    private static void EnsureGridViewHeaderStyle(GridView gv)
    {
        var baseStyle = gv.ColumnHeaderContainerStyle;
        if (baseStyle != null && baseStyle.TargetType != typeof(GridViewColumnHeader))
        {
            return;
        }

        var style = new Style(typeof(GridViewColumnHeader), baseStyle);
        style.Setters.Add(new Setter(FilterableColumnHeaderBehavior.IsFilterableProperty, true));
        gv.ColumnHeaderContainerStyle = style;
    }

    #endregion

    #region ViewModel attached property

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.RegisterAttached(
            "ViewModel",
            typeof(object),
            typeof(FilterableGridAttach),
            new PropertyMetadata(null));

    public static object? GetViewModel(DependencyObject obj) => obj.GetValue(ViewModelProperty);
    public static void SetViewModel(DependencyObject obj, object? value) => obj.SetValue(ViewModelProperty, value);

    #endregion
}

