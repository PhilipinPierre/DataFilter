namespace UIContracts.Common;

/// <summary>
/// Cross-framework UI contract identifiers (see TestUIContract.md / UIMatrixContract.md).
/// </summary>
public enum ContractScenario
{
    PopupOpenClose,
    AnchoredPositioning,
    ScrollKeepsPopupAnchored,
    FilteringSingleColumn,
    FilteringMultiColumn,
    AddToExistingUnion,
    AddToExistingIntersection,
    ClearFilters,
    SortSimple,
    SortMultiKey,
    SortAfterFilter,
    Localization,
    FilterPipelineJson,
    OutsideClickDoesNotClickThrough,
    ResizeBehavior,
    RtlLayout
}
