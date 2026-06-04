namespace UIContracts.Common;

/// <summary>
/// Demo integration views/routes per host stack for UI contract navigation.
/// </summary>
public static class DemoViewCatalog
{
    public static class Blazor
    {
        public const string Attach = "/demo/attach";
        public const string Local = "/demo/local";
        public const string Async = "/demo/async";
        public const string Hybrid = "/demo/hybrid";
        public const string CollectionView = "/demo/collectionview";
        public const string Customization = "/demo/customization";
    }

    public static class Wpf
    {
        public const string LocalTab = "Local Filtering";
        public const string AttachTab = "Attach (DataGrid)";
        public const string AsyncTab = "Async Filtering";
        public const string HybridTab = "Hybrid Filtering";
        public const string ListViewTab = "ListView Example";
        public const string CollectionViewTab = "CollectionView Example";
        public const string CustomizationTab = "Customization";
    }

    public static class WinForms
    {
        public const string LocalTab = "Local Filtering";
        public const string AttachTab = "Attach (DataGridView)";
        public const string AsyncTab = "Async Filtering";
        public const string HybridTab = "Hybrid Filtering";
        public const string ListViewTab = "ListView Example";
        public const string CollectionViewTab = "CollectionView Example";
        public const string CustomizationTab = "Customization";
    }

    public static class WinUi3
    {
        public const string LocalNav = "Local";
        public const string AttachNav = "Attach (ListView)";
        public const string AsyncNav = "Async";
        public const string HybridNav = "Hybrid";
        public const string ListViewNav = "ListView";
        public const string CollectionViewNav = "CollectionView";
        public const string CustomizationNav = "Customization";
    }

    public static class Maui
    {
        public const string LocalRoute = "LocalFilterPage";
        public const string AttachRoute = "AttachFilterPage";
        public const string AsyncRoute = "AsyncFilterPage";
        public const string HybridRoute = "HybridFilterPage";
        public const string CollectionViewRoute = "CollectionViewPage";
        public const string CustomizationRoute = "CustomizationPage";
    }
}
