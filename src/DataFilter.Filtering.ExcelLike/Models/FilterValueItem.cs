using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace DataFilter.Filtering.ExcelLike.Models;

/// <summary>
/// Represents an item or group in the filter checkbox tree list.
/// </summary>
public partial class FilterValueItem : ObservableObject
{
    private bool _isUpdating;

    [ObservableProperty]
    private object? _value;

    [ObservableProperty]
    private bool? _isSelected;

    [ObservableProperty]
    private bool? _isNull;

    // UI state for tree-like displays (e.g. Blazor date filter popup).
    // Default is collapsed to match WPF TreeView's IsExpanded=false behavior.
    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private string _displayText;

    public ObservableCollection<FilterValueItem> Children { get; } = new();

    public FilterValueItem? Parent { get; }

    public FilterValueItem(string displayText, object? value, FilterValueItem? parent = null, bool isSelected = true)
    {
        _displayText = displayText;
        _value = value;
        Parent = parent;
        _isSelected = isSelected;
        _isNull = false;
        _isExpanded = false;
    }

    public void AddChild(FilterValueItem child)
    {
        Children.Add(child);
        child.PropertyChanged += Child_PropertyChanged;
    }

    private void Child_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsSelected) && !_isUpdating)
        {
            UpdateStateFromChildren();
        }
    }

    partial void OnIsSelectedChanged(bool? value)
    {
        if (_isUpdating) return;

        bool targetValue = false;
        if (!value.HasValue)
        {
            _isUpdating = true;
            IsSelected = false;
            IsNull = true;
            targetValue = false;
        }
        else
        {
            _isUpdating = true;
            targetValue = value.Value;
            IsNull = false;
        }

        foreach (var child in Children)
        {
            child.IsSelected = targetValue;
        }
        _isUpdating = false;

        Parent?.UpdateStateFromChildren();
    }

    public void UpdateStateFromChildren()
    {
        if (_isUpdating || Children.Count == 0) return;

        _isUpdating = true;
        
        bool allSelected = Children.All(c => c.IsSelected == true);
        bool allUnselected = Children.All(c => c.IsSelected == false);

        if (allSelected)
            IsSelected = true;
        else if (allUnselected)
            IsSelected = false;
        else
            IsSelected = null;
            
        _isUpdating = false;

        Parent?.UpdateStateFromChildren();
    }

    public void ToggleExpanded() => IsExpanded = !IsExpanded;

    public void GetSelectedValues(HashSet<object> selectedValues)
    {
        if (Children.Count == 0)
        {
            if (IsSelected == true && Value != null)
            {
                selectedValues.Add(Value);
            }
        }
        else
        {
            foreach (var child in Children)
            {
                child.GetSelectedValues(selectedValues);
            }
        }
    }

    public void GetSelectedValues(ConcurrentQueue<object> selectedValues)
    {
        if (Children.Count == 0)
        {
            if (IsSelected == true && Value != null)
            {
                selectedValues.Enqueue(Value);
            }
        }
        else
        {
            foreach (var child in Children)
            {
                child.GetSelectedValues(selectedValues);
            }
        }
    }
}
