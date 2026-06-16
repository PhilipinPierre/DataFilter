namespace DataFilter.WinForms;

/// <summary>
/// Closes registered popups when the WinForms application loses activation to another program.
/// </summary>
public static class AppDeactivationTracker
{
    private static readonly List<Action> CloseCallbacks = new();
    private static AppDeactivationMessageFilter? _filter;

    public static void Register(Action close)
    {
        ArgumentNullException.ThrowIfNull(close);

        if (CloseCallbacks.Count == 0)
        {
            _filter = new AppDeactivationMessageFilter(OnApplicationDeactivated);
            Application.AddMessageFilter(_filter);
        }

        if (!CloseCallbacks.Contains(close))
            CloseCallbacks.Add(close);
    }

    public static void Unregister(Action close)
    {
        ArgumentNullException.ThrowIfNull(close);

        CloseCallbacks.Remove(close);
        if (CloseCallbacks.Count != 0 || _filter == null)
            return;

        Application.RemoveMessageFilter(_filter);
        _filter = null;
    }

    private static void OnApplicationDeactivated()
    {
        foreach (var close in CloseCallbacks.ToArray())
            close();
    }

    private sealed class AppDeactivationMessageFilter(Action onApplicationDeactivated) : IMessageFilter
    {
        private const int WmActivateApp = 0x001C;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WmActivateApp && m.WParam == IntPtr.Zero)
                onApplicationDeactivated();

            return false;
        }
    }
}
