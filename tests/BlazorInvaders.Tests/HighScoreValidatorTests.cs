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
