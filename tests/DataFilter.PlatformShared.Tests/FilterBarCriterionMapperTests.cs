using System.Globalization;
using DataFilter.Core.Enums;
using DataFilter.Core.Pipeline;
using DataFilter.Filtering.ExcelLike.Models;
using DataFilter.Localization;
using DataFilter.PlatformShared.FilterBar;

namespace DataFilter.PlatformShared.Tests;

public class FilterBarCriterionMapperTests
{
    [Fact]
    public void ApplyStateToCriterion_Maps_In_List_From_Excel_State()
    {
        var node = new CriterionPipelineNode { PropertyName = "Name" };
        var state = new ExcelFilterState { SelectAll = false };
        state.SelectedValues.Add("Alice");
        state.SelectedValues.Add("Bob");

        FilterBarCriterionMapper.ApplyStateToCriterion(node, "Name", state);

        Assert.Equal(nameof(FilterOperator.In), node.Operator);
        var values = Assert.IsAssignableFrom<IEnumerable<object>>(node.Value);
        Assert.Contains("Alice", values.Cast<object?>().Select(v => v?.ToString()));
    }

    [Fact]
    public void ResolveStateForEdit_Uses_Column_State_When_Node_Is_Empty()
    {
        var node = new CriterionPipelineNode
        {
            PropertyName = "Name",
            Operator = nameof(FilterOperator.Equals),
            Value = null
        };
        var columnState = new ExcelFilterState { SelectAll = false };
        columnState.SelectedValues.Add("Zoe");

        var resolved = FilterBarCriterionMapper.ResolveStateForEdit(node, columnState);

        Assert.False(resolved.SelectAll);
        Assert.Contains("Zoe", resolved.SelectedValues);
    }

    [Fact]
    public void Format_Uses_Inline_Lowercase_Operator_After_Column()
    {
        var previous = LocalizationManager.Instance.Culture;
        try
        {
            LocalizationManager.Instance.SetCulture(new CultureInfo("fr-FR"));
            var criterion = new CriterionPipelineNode
            {
                PropertyName = "Name",
                Operator = nameof(FilterOperator.Equals),
                Value = "Alice"
            };

            string text = FilterCriterionFormatter.Format(criterion);

            Assert.StartsWith("name ", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("égal à", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Égal à", text);
        }
        finally
        {
            LocalizationManager.Instance.SetCulture(previous);
        }
    }
}
