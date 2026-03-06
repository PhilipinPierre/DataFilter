using Xunit;
using DataFilter.Filtering.ExcelLike.Services;

namespace DataFilter.Filtering.ExcelLike.Tests;

public class WildcardMatcherTests
{
    private readonly WildcardMatcher _matcher = new();

    [Fact]
    public void IsMatch_Asterisk_MatchesAnySuffix()
    {
        Assert.True(_matcher.IsMatch("Grand-Nord", "*nord"));
        Assert.True(_matcher.IsMatch("Sud-Nord", "*nord"));
        Assert.False(_matcher.IsMatch("Grand-Sud", "*nord"));
    }

    [Fact]
    public void IsMatch_Asterisk_MatchesAnyPrefix()
    {
        Assert.True(_matcher.IsMatch("port", "p*t"));
        Assert.True(_matcher.IsMatch("part", "p*t"));
        Assert.False(_matcher.IsMatch("par", "p*t"));
    }

    [Fact]
    public void IsMatch_QuestionMark_MatchesSingleCharacter()
    {
        Assert.True(_matcher.IsMatch("part", "p?rt"));
        Assert.True(_matcher.IsMatch("port", "p?rt"));
        Assert.False(_matcher.IsMatch("part", "p?t"));
    }

    [Fact]
    public void IsMatch_IsCaseInsensitive()
    {
        Assert.True(_matcher.IsMatch("ALICE", "*lice"));
        Assert.True(_matcher.IsMatch("alice", "*LICE"));
    }

    [Fact]
    public void ContainsWildcard_DetectsAsteriskAndQuestionMark()
    {
        Assert.True(_matcher.ContainsWildcard("test*"));
        Assert.True(_matcher.ContainsWildcard("te?t"));
        Assert.False(_matcher.ContainsWildcard("test"));
    }

    [Fact]
    public void IsMatch_NoWildcard_ActsLikeExactMatch()
    {
        Assert.True(_matcher.IsMatch("alice", "alice"));
        Assert.False(_matcher.IsMatch("alice", "bob"));
    }
}
