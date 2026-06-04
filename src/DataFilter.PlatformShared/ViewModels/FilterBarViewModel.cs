using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataFilter.Core.Pipeline;
using DataFilter.Core.Services;
using DataFilter.Localization;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.Pipeline;

namespace DataFilter.PlatformShared.ViewModels;

/// <summary>
/// View model for the active-filters bar (chips, AND/OR layout, commands).
/// </summary>
public partial class FilterBarViewModel : ObservableObject
{
    private readonly FilterPipelineSession _session;
    private Func<string, string>? _resolveColumnTitle;
    private Func<string>? _resolveDefaultPropertyName;
    private Func<FilterPipeline, Task>? _applyPipelineAsync;

    public FilterBarViewModel(FilterPipelineSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _session.PipelineChanged += (_, _) => RebuildDisplay();
        LocalizationManager.Instance.CultureChanged += (_, _) => RebuildDisplay();
    }

    public ObservableCollection<FilterBarDisplayItem> Segments { get; } = new();

    /// <summary>
    /// Raised when a chip should open the column filter popup.
    /// </summary>
    public event EventHandler<FilterBarEditRequest>? EditRequested;

    /// <summary>
    /// Raised when <see cref="Segments"/> were rebuilt (pipeline, culture, or layout).
    /// </summary>
    public event EventHandler? DisplayChanged;

    /// <summary>
    /// Configures column title resolution and pipeline apply callback from the grid host.
    /// </summary>
    public void Configure(
        Func<string, string>? resolveColumnTitle,
        Func<FilterPipeline, Task> applyPipelineAsync,
        Func<string>? resolveDefaultPropertyName = null)
    {
        _resolveColumnTitle = resolveColumnTitle;
        _resolveDefaultPropertyName = resolveDefaultPropertyName;
        _applyPipelineAsync = applyPipelineAsync ?? throw new ArgumentNullException(nameof(applyPipelineAsync));
    }

    public void RebuildDisplay()
    {
        Segments.Clear();
        foreach (FilterBarDisplayItem item in FilterBarDisplayBuilder.Build(_session.Pipeline, _resolveColumnTitle))
            Segments.Add(item);

        DisplayChanged?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task ToggleEnabledAsync(string nodeId)
    {
        if (!_session.TryGetNode(nodeId, out FilterPipelineNode? node) || node == null)
            return;

        _session.SetEnabled(nodeId, !node.IsEnabled);
        await ApplyPipelineAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task RemoveAsync(string nodeId)
    {
        if (!_session.RemoveNode(nodeId))
            return;

        await ApplyPipelineAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private void Edit(string nodeId)
    {
        if (!_session.TryGetNode(nodeId, out FilterPipelineNode? node) || node == null)
            return;

        string? propertyName = FilterPipelineEditor.TryResolveColumnPropertyName(node);
        if (string.IsNullOrWhiteSpace(propertyName))
            return;

        EditRequested?.Invoke(this, new FilterBarEditRequest { NodeId = nodeId, PropertyName = propertyName });
    }

    [RelayCommand]
    private void AddOrGroup()
    {
        string? propertyName = _resolveDefaultPropertyName?.Invoke();
        if (string.IsNullOrWhiteSpace(propertyName))
            return;

        CriterionPipelineNode? created = _session.AddOrGroup(propertyName);
        if (created == null)
            return;

        RebuildDisplay();
        EditRequested?.Invoke(this, new FilterBarEditRequest
        {
            NodeId = created.Id,
            PropertyName = created.PropertyName,
            IsNew = true
        });
    }

    [RelayCommand]
    private async Task MoveToClusterAsync((string CriterionNodeId, string TargetClusterAnchorNodeId) args)
    {
        if (string.IsNullOrEmpty(args.CriterionNodeId) || string.IsNullOrEmpty(args.TargetClusterAnchorNodeId))
            return;

        if (!_session.MoveCriterionToCluster(args.CriterionNodeId, args.TargetClusterAnchorNodeId))
            return;

        await ApplyPipelineAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task MoveToOrGapAsync((string CriterionNodeId, int OrInsertIndex) args)
    {
        if (string.IsNullOrEmpty(args.CriterionNodeId))
            return;

        if (!_session.MoveCriterionToOrGap(args.CriterionNodeId, args.OrInsertIndex))
            return;

        await ApplyPipelineAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task AddAndAsync(string anchorNodeId)
    {
        CriterionPipelineNode? created = _session.AddAndCriterion(anchorNodeId);
        if (created == null)
            return;

        // Same column as anchor; do not apply until the popup confirms the rule.
        RebuildDisplay();
        EditRequested?.Invoke(this, new FilterBarEditRequest
        {
            NodeId = created.Id,
            PropertyName = created.PropertyName,
            IsNew = true
        });
    }

    private async Task ApplyPipelineAsync()
    {
        if (_applyPipelineAsync != null)
            await _applyPipelineAsync(_session.Pipeline).ConfigureAwait(false);
        RebuildDisplay();
    }
}
