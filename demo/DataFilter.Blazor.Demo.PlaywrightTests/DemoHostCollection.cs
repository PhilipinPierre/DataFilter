namespace DataFilter.Blazor.Demo.PlaywrightTests;

[CollectionDefinition(Name)]
public sealed class DemoHostCollection : ICollectionFixture<DemoHostFixture>
{
    public const string Name = "DemoHost";
}

