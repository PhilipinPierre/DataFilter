using DataFilter.PlatformShared.ColumnFilter;

namespace DataFilter.PlatformShared.Tests;

public sealed class ColumnFilterPopupCoordinatorTests
{
    [Fact]
    public void NotifyOpened_closes_other_popups_in_same_group()
    {
        var coordinator = new ColumnFilterPopupCoordinator();
        var group = new object();
        var ownerA = new object();
        var ownerB = new object();
        var closedA = false;

        coordinator.NotifyOpened(group, ownerA, () => closedA = true);
        coordinator.NotifyOpened(group, ownerB, () => { });

        Assert.True(closedA);
    }

    [Fact]
    public void NotifyClosed_removes_owner_without_closing_others()
    {
        var coordinator = new ColumnFilterPopupCoordinator();
        var group = new object();
        var ownerA = new object();
        var ownerB = new object();
        var closedB = false;

        coordinator.NotifyOpened(group, ownerA, () => { });
        coordinator.NotifyOpened(group, ownerB, () => closedB = true);
        coordinator.NotifyClosed(group, ownerB);

        coordinator.NotifyOpened(group, ownerA, () => { });

        Assert.False(closedB);
    }
}
