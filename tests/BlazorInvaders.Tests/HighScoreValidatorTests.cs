using System.Text;
using BlazorInvaders.Api;
using Xunit;

namespace BlazorInvaders.Tests;

public class HighScoreValidatorTests
{
    private static bool Parse(string json, out HighScoreRequest score, out string error) =>
        HighScoreValidator.TryParse(Encoding.UTF8.GetBytes(json), out score, out error);

    [Theory]
    [InlineData("abc", "ABC")]
    [InlineData(" Zx ", "ZX")]
    [InlineData("7", "7")]
    public void ValidName_IsNormalized(string name, string expected)
    {
        Assert.True(Parse($$"""{"name":"{{name}}","score":100}""", out var score, out _));
        Assert.Equal(expected, score.Name);
        Assert.Equal(100, score.Score);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABCD")]
    [InlineData("\\u0000\\u0000\\u0000")]
    [InlineData("<b>")]
    [InlineData("ÅÄÖ")]
    public void InvalidName_IsRejected(string name)
    {
        Assert.False(Parse($$"""{"name":"{{name}}","score":100}""", out _, out var error));
        Assert.NotEmpty(error);
    }

    [Fact]
    public void MissingName_IsRejected() =>
        Assert.False(Parse("""{"score":100}""", out _, out _));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(HighScoreValidator.MaxScore + 1)]
    [InlineData(int.MaxValue)]
    public void OutOfRangeScore_IsRejected(int value) =>
        Assert.False(Parse($$"""{"name":"ABC","score":{{value}}}""", out _, out _));

    [Fact]
    public void MaxScore_IsAccepted() =>
        Assert.True(Parse($$"""{"name":"ABC","score":{{HighScoreValidator.MaxScore}}}""", out _, out _));

    [Theory]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("""{"name":"ABC","score":1e20}""")]
    public void MalformedBody_IsRejected(string json) =>
        Assert.False(Parse(json, out _, out _));

    private static readonly DateOnly Today = new(2026, 10, 13);

    private static bool ParseOn(string json, out HighScoreRequest score, out string error) =>
        HighScoreValidator.TryParse(Encoding.UTF8.GetBytes(json), Today, out score, out error);

    [Fact]
    public void AllTimeScore_GoesToAllTimePartition()
    {
        Assert.True(ParseOn("""{"name":"ABC","score":100}""", out var score, out _));
        Assert.Null(score.Mode);
        Assert.Equal("HighScore", HighScoreValidator.PartitionFor(score));
    }

    [Theory]
    [InlineData("20261013")]    // today
    [InlineData("20261012")]    // yesterday: a game that crossed midnight
    public void DailyScore_ForTodayOrYesterday_IsAccepted(string day)
    {
        Assert.True(ParseOn($$"""{"name":"ABC","score":100,"mode":"daily","day":"{{day}}"}""", out var score, out _));
        Assert.Equal("daily", score.Mode);
        Assert.Equal($"Daily-{day}", HighScoreValidator.PartitionFor(score));
    }

    [Theory]
    [InlineData("20261011")]    // two days ago
    [InlineData("20261014")]    // tomorrow
    [InlineData("2026-10-13")]
    [InlineData("20261399")]
    [InlineData("")]
    [InlineData("x' or 'a'='a")]
    public void DailyScore_ForOtherDays_IsRejected(string day) =>
        Assert.False(ParseOn($$"""{"name":"ABC","score":100,"mode":"daily","day":"{{day}}"}""", out _, out _));

    [Fact]
    public void DailyScore_WithoutDay_IsRejected() =>
        Assert.False(ParseOn("""{"name":"ABC","score":100,"mode":"daily"}""", out _, out _));

    [Fact]
    public void UnknownMode_IsRejected() =>
        Assert.False(ParseOn("""{"name":"ABC","score":100,"mode":"weekly","day":"20261013"}""", out _, out _));

    [Theory]
    [InlineData(null, null, "HighScore")]
    [InlineData("", null, "HighScore")]
    [InlineData("daily", null, "Daily-20261013")]
    [InlineData("daily", "20261001", "Daily-20261001")]
    public void LeaderboardQuery_ResolvesPartition(string? mode, string? day, string expected)
    {
        Assert.True(HighScoreValidator.TryGetLeaderboardPartition(mode, day, Today, out var partition, out _));
        Assert.Equal(expected, partition);
    }

    [Theory]
    [InlineData("weekly", null)]
    [InlineData("daily", "today")]
    [InlineData("daily", "x' or PartitionKey ne '")]
    public void LeaderboardQuery_WithBadParameters_IsRejected(string mode, string? day) =>
        Assert.False(HighScoreValidator.TryGetLeaderboardPartition(mode, day, Today, out _, out _));

    [Fact]
    public async Task ReadBounded_UnderLimit_ReturnsBytes()
    {
        var bytes = await HighScoreValidator.ReadBoundedAsync(new MemoryStream(new byte[100]), 1024);
        Assert.Equal(100, bytes!.Length);
    }

    [Fact]
    public async Task ReadBounded_OverLimit_ReturnsNull()
    {
        var bytes = await HighScoreValidator.ReadBoundedAsync(new MemoryStream(new byte[2000]), 1024);
        Assert.Null(bytes);
    }
}
