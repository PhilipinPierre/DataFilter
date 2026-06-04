namespace UIContracts.Common;

/// <summary>
/// Describes a single-column filter scenario for parameterized UI contract tests.
/// </summary>
public sealed record ColumnFilterCase(
    string PropertyName,
    string UiOperator,
    string FilterValue,
    string InvariantDescription);
