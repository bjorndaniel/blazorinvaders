using System.Net;
using System.Text.Json;
using BlazorInvaders.Api;
using BlazorInvaders.GameObjects;
using Xunit;

namespace BlazorInvaders.Tests;

public class HighScoreTests
{
    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, true)]
    [InlineData(HttpStatusCode.Forbidden, true)]
    [InlineData(HttpStatusCode.OK, false)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    [InlineData(HttpStatusCode.RequestEntityTooLarge, false)]
    [InlineData(HttpStatusCode.InternalServerError, false)]
    public void ShouldRetrySend_OnlyForTemporaryRefusals(HttpStatusCode status, bool expected) =>
        Assert.Equal(expected, HighScore.ShouldRetrySend(status));

    [Fact]
    public void Wave_IsSerializedAndAcceptedByValidator()
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(new HighScore { Name = "ABC", Score = 100, Wave = 3 });
        Assert.Contains("\"wave\":3", System.Text.Encoding.UTF8.GetString(json));

        Assert.True(HighScoreValidator.TryParse(json, out var score, out _));
        Assert.Equal(3, score.Wave);
    }

    [Fact]
    public void Wave_IsOmittedWhenZero() =>
        Assert.DoesNotContain("wave", JsonSerializer.Serialize(new HighScore { Name = "ABC", Score = 100 }));
}
