namespace DataFilter.PlatformShared.ColumnFilter;

/// <summary>
/// Tracks open column-filter popups per host (grid) so only one stays open and siblings can be closed together.
/// </summary>
public sealed class ColumnFilterPopupCoordinator
{
    public static ColumnFilterPopupCoordinator Instance { get; } = new();

    private readonly object _sync = new();
    private readonly Dictionary<object, List<Entry>> _groups = new();

    private sealed class Entry
    {
        public required object Owner { get; init; }
        public required Action Close { get; init; }
    }

    public void NotifyOpened(object groupKey, object owner, Action close)
    {
        ArgumentNullException.ThrowIfNull(groupKey);
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(close);

        lock (_sync)
        {
            var list = GetOrCreateList(groupKey);
            foreach (var entry in list.ToList())
            {
                if (!ReferenceEquals(entry.Owner, owner))
                    entry.Close();
            }

            list.RemoveAll(e => ReferenceEquals(e.Owner, owner));
            list.Add(new Entry { Owner = owner, Close = close });
        }
    }

    public void NotifyClosed(object groupKey, object owner)
    {
        ArgumentNullException.ThrowIfNull(groupKey);
        ArgumentNullException.ThrowIfNull(owner);

        lock (_sync)
        {
            if (!_groups.TryGetValue(groupKey, out var list))
                return;

            list.RemoveAll(e => ReferenceEquals(e.Owner, owner));
            if (list.Count == 0)
                _groups.Remove(groupKey);
        }
    }

    public void CloseAll(object groupKey)
    {
        ArgumentNullException.ThrowIfNull(groupKey);

        lock (_sync)
        {
            if (!_groups.TryGetValue(groupKey, out var list))
                return;

            foreach (var entry in list.ToList())
                entry.Close();

            list.Clear();
            _groups.Remove(groupKey);
        }
    }

    private List<Entry> GetOrCreateList(object groupKey)
    {
        if (!_groups.TryGetValue(groupKey, out var list))
        {
            list = new List<Entry>();
            _groups[groupKey] = list;
        }

        return list;
    }
}
