namespace DataFilter.Core.Abstractions;

/// <summary>
/// Extracts and restores filter snapshots from/to a <see cref="IFilterContext"/>.
/// Use this to implement client-side persistence of filter configurations:
/// <list type="number">
///   <item>Call <see cref="CreateSnapshot"/> to get a snapshot of the current filters.</item>
///   <item>Serialize the snapshot using your preferred serialization framework (e.g. System.Text.Json, Newtonsoft.Json, XML).</item>
///   <item>Persist the serialized string (file, database, user settings, etc.).</item>
///   <item>To restore: deserialize back to <see cref="IFilterSnapshot"/> and call <see cref="RestoreSnapshot"/>.</item>
/// </list>
/// </summary>
public interface IFilterSnapshotBuilder
{
    /// <summary>
    /// Creates an immutable snapshot of all active filters and sort criteria
    /// from the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The filter context to snapshot.</param>
    /// <returns>A snapshot containing serialization-friendly POCOs.</returns>
    IFilterSnapshot CreateSnapshot(IFilterContext context);

    /// <summary>
    /// Restores filters and sort criteria from a <paramref name="snapshot"/>
    /// into the given <paramref name="context"/>, replacing any existing state.
    /// </summary>
    /// <param name="context">The filter context to restore into.</param>
    /// <param name="snapshot">The snapshot to restore from.</param>
    void RestoreSnapshot(IFilterContext context, IFilterSnapshot snapshot);
}
