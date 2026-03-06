namespace DataFilter.Core.Enums;

/// <summary>
/// Specifies the operator used for filtering.
/// </summary>
public enum FilterOperator
{
    Contains,
    NotContains,
    StartsWith,
    EndsWith,
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Between,
    In,
    NotIn,
    IsNull,
    IsNotNull
}
