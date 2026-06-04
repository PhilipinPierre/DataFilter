using DataFilter.Localization;
using DataFilter.PlatformShared.FilterBar;
using DataFilter.PlatformShared.ViewModels;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DataFilter.WinForms.Controls;

/// <summary>
/// WinForms active-filters bar.
/// </summary>
public sealed class FilterBarControl : Panel
{
    private readonly FlowLayoutPanel _flow = new() { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true };
    private FilterBarViewModel? _viewModel;
    private IFilterableDataGridViewModel? _gridViewModel;
    private Action<FilterBarEditRequest>? _onEdit;

    public FilterBarControl()
    {
        Dock = DockStyle.Top;
        Height = 40;
        Visible = false;
        AccessibleName = "df-filter-bar";
        Controls.Add(_flow);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterBarViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (_viewModel != null)
                _viewModel.EditRequested -= OnEditRequested;
            _viewModel = value;
            if (_viewModel != null)
                _viewModel.EditRequested += OnEditRequested;
            RebuildUi();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFilterableDataGridViewModel? GridViewModel
    {
        get => _gridViewModel;
        set
        {
            if (_gridViewModel?.PipelineSession != null)
                _gridViewModel.PipelineSession.PipelineChanged -= OnPipelineChanged;
            _gridViewModel = value;
            if (_gridViewModel?.PipelineSession != null)
                _gridViewModel.PipelineSession.PipelineChanged += OnPipelineChanged;
            ViewModel = value?.FilterBar;
        }
    }

    private void OnPipelineChanged(object? sender, EventArgs e) => RebuildUi();

    [DefaultValue(false)]
    public bool ShowFilterBar
    {
        get => Visible;
        set => Visible = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Action<FilterBarEditRequest>? OnEditRequestedHandler
    {
        get => _onEdit;
        set => _onEdit = value;
    }

    public void RebuildUi()
    {
        _flow.SuspendLayout();
        _flow.Controls.Clear();
        if (_viewModel == null)
        {
            _flow.ResumeLayout();
            return;
        }

        foreach (FilterBarDisplayItem segment in _viewModel.Segments)
        {
            if (segment is FilterBarOrSeparatorItem orSep)
            {
                _flow.Controls.Add(new Label
                {
                    Text = orSep.Text,
                    AutoSize = true,
                    Margin = new Padding(8, 6, 4, 4),
                    Font = new Font(Font, FontStyle.Bold)
                });
            }
            else if (segment is FilterBarAndClusterItem cluster)
            {
                var clusterPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    WrapContents = true,
                    Margin = new Padding(4),
                    Padding = new Padding(4),
                    BorderStyle = BorderStyle.FixedSingle
                };
                if (cluster.CanAddAnd && !string.IsNullOrEmpty(cluster.AddAndAnchorNodeId))
                    clusterPanel.Controls.Add(CreateAddButton(cluster.AddAndAnchorNodeId));

                foreach (FilterBarChipItem chip in cluster.Chips)
                    clusterPanel.Controls.Add(CreateChipPanel(chip));

                _flow.Controls.Add(clusterPanel);
            }
        }

        _flow.ResumeLayout();
    }

    private Panel CreateChipPanel(FilterBarChipItem chip)
    {
        var panel = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(2) };
        var label = new Button
        {
            Text = chip.DisplayText,
            AutoSize = true,
            FlatStyle = FlatStyle.Flat,
            Enabled = chip.IsEnabled,
            AccessibleName = $"df-filter-bar-chip-{chip.NodeId}"
        };
        label.Click += (_, _) => _onEdit?.Invoke(new FilterBarEditRequest { NodeId = chip.NodeId, PropertyName = chip.PropertyName });
        label.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Right && _viewModel?.ToggleEnabledCommand.CanExecute(chip.NodeId) == true)
                _viewModel.ToggleEnabledCommand.Execute(chip.NodeId);
        };
        panel.Controls.Add(label);
        if (chip.CanAddAnd)
            panel.Controls.Add(CreateAddButton(chip.NodeId));
        var remove = new Button { Text = "×", Width = 24, FlatStyle = FlatStyle.Flat };
        remove.Click += (_, _) => _viewModel?.RemoveCommand.Execute(chip.NodeId);
        panel.Controls.Add(remove);
        return panel;
    }

    private Button CreateAddButton(string nodeId)
    {
        var btn = new Button
        {
            Text = "+",
            Width = 24,
            FlatStyle = FlatStyle.Flat,
            AccessibleName = $"df-filter-bar-add-{nodeId}"
        };
        btn.Click += (_, _) => _viewModel?.AddAndCommand.Execute(nodeId);
        return btn;
    }

    private void OnEditRequested(object? sender, FilterBarEditRequest e) => _onEdit?.Invoke(e);
}
