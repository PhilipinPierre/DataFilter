using System.Windows;
using System.Windows.Controls;
using DataFilter.PlatformShared.FilterValues;

namespace DataFilter.Wpf.Controls;

/// <summary>
/// Simple time-of-day picker bound to an invariant <c>HH:mm:ss</c> string.
/// </summary>
public partial class FilterTimePicker : UserControl
{
    public static readonly DependencyProperty TimeTextProperty =
        DependencyProperty.Register(
            nameof(TimeText),
            typeof(string),
            typeof(FilterTimePicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeTextChanged));

    private bool _internalUpdate;

    public FilterTimePicker()
    {
        InitializeComponent();
        PopulatePart(HourBox, 0, 23);
        PopulatePart(MinuteBox, 0, 59);
        PopulatePart(SecondBox, 0, 59);
    }

    public string TimeText
    {
        get => (string)GetValue(TimeTextProperty);
        set => SetValue(TimeTextProperty, value);
    }

    private static void OnTimeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterTimePicker picker)
            picker.ApplyTimeTextToParts();
    }

    private void OnPartChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_internalUpdate)
            return;

        if (HourBox.SelectedItem is not int hour
            || MinuteBox.SelectedItem is not int minute
            || SecondBox.SelectedItem is not int second)
            return;

        TimeText = FilterCustomValueFormats.FormatTime(new TimeSpan(hour, minute, second));
    }

    private void ApplyTimeTextToParts()
    {
        _internalUpdate = true;
        try
        {
            if (FilterCustomValueFormats.TryParseTime(TimeText, out var time))
            {
                SelectPart(HourBox, time.Hours);
                SelectPart(MinuteBox, time.Minutes);
                SelectPart(SecondBox, time.Seconds);
            }
            else
            {
                HourBox.SelectedIndex = -1;
                MinuteBox.SelectedIndex = -1;
                SecondBox.SelectedIndex = -1;
            }
        }
        finally
        {
            _internalUpdate = false;
        }
    }

    private static void PopulatePart(ComboBox comboBox, int min, int max)
    {
        for (int i = min; i <= max; i++)
            comboBox.Items.Add(i);
    }

    private static void SelectPart(ComboBox comboBox, int value)
    {
        for (int i = 0; i < comboBox.Items.Count; i++)
        {
            if (comboBox.Items[i] is int item && item == value)
            {
                comboBox.SelectedIndex = i;
                return;
            }
        }

        comboBox.SelectedIndex = -1;
    }
}
