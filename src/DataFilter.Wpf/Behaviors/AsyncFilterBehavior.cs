using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DataFilter.Wpf.Behaviors;

/// <summary>
/// A behavior that delays the update of a text binding (debounce) and handles async filtering.
/// Useful for search boxes where typing should trigger an async API call after a short delay.
/// </summary>
public class AsyncFilterBehavior : Behavior<TextBox>
{
    private DispatcherTimer? _timer;

    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.Register(nameof(Delay), typeof(TimeSpan), typeof(AsyncFilterBehavior), new PropertyMetadata(TimeSpan.FromMilliseconds(300)));

    /// <summary>
    /// Gets or sets the delay before firing the command.
    /// </summary>
    public TimeSpan Delay
    {
        get => (TimeSpan)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(AsyncFilterBehavior), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command to execute after the delay.
    /// </summary>
    public ICommand SearchCommand
    {
        get => (ICommand)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        _timer = new DispatcherTimer
        {
            Interval = Delay
        };
        _timer.Tick += Timer_Tick;

        AssociatedObject.TextChanged += AssociatedObject_TextChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }

        AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
    }

    private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
    {
        _timer?.Stop();

        // Update the bound property explicitly to ensure the ViewModel sees the latest text
        // before the command fires. We assume UpdateSourceTrigger=PropertyChanged is set, 
        // but just in case, this resets the debounce timer.
        _timer?.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _timer?.Stop();

        if (SearchCommand?.CanExecute(AssociatedObject.Text) == true)
        {
            SearchCommand.Execute(AssociatedObject.Text);
        }
    }
}
