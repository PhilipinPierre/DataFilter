using DataFilter.Core.Engine;

namespace DataFilter.Core.Tests;

public class TimeDistinctHelperTests
{
    [Fact]
    public void AreSameTimeOfDay_TimeSpan_MatchesExactValue()
    {
        var left = new TimeSpan(8, 15, 30, 500);
        var right = new TimeSpan(8, 15, 30, 500);

        Assert.True(TimeDistinctHelper.AreSameTimeOfDay(left, right));
        Assert.False(TimeDistinctHelper.AreSameTimeOfDay(left, new TimeSpan(8, 15, 30, 501)));
    }

    [Fact]
    public void CanonicalizeDistinctValue_DateTimeOffset_ReturnsTimeSpanForTimeSpanColumn()
    {
        var value = new DateTimeOffset(2024, 3, 15, 8, 15, 30, TimeSpan.Zero);

        var canonical = TimeDistinctHelper.CanonicalizeDistinctValue(value, typeof(TimeSpan));

        Assert.Equal(new TimeSpan(8, 15, 30), canonical);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void CanonicalizeDistinctValue_TimeSpan_ReturnsTimeOnlyForTimeOnlyColumn()
    {
        var value = new TimeSpan(9, 30, 15);

        var canonical = (TimeOnly)TimeDistinctHelper.CanonicalizeDistinctValue(value, typeof(TimeOnly));

        Assert.Equal(new TimeOnly(9, 30, 15), canonical);
    }
#endif
}
