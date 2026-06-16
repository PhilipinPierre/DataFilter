using DataFilter.WinForms;

namespace DataFilter.WinForms.Tests;

public sealed class AppDeactivationTrackerTests
{
    [Fact]
    public void Register_and_unregister_do_not_throw()
    {
        void Close() { }

        AppDeactivationTracker.Register(Close);
        AppDeactivationTracker.Unregister(Close);
    }
}
