using BlazorInvaders.Api;
using Xunit;

namespace BlazorInvaders.Tests;

public class LeaderboardRankingTests
{
    private const string P = "HighScore";

    [Fact]
    public void RankedRowKey_OrdinalSort_GivesScoresDescending()
    {
        var scores = new[] { 9, 10, 999_999, 1_000_000, 1, 0, int.MaxValue, -5 };

        var sorted = scores
            .Select(s => LeaderboardRanking.RankedRowKey(s, "x"))
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();

        var expected = scores.OrderByDescending(s => s).Select(s => LeaderboardRanking.RankedRowKey(s, "x"));
        Assert.Equal(expected, sorted);
    }

    [Fact]
    public void RankedRowKey_MixedDigitCounts_StillSortDescending()
    {
        var scores = new[] { 2_000_000_000, 1_000_000_000, 1_147_483_647, 5 };

        var sorted = scores
            .Select(s => LeaderboardRanking.RankedRowKey(s, "x"))
            .OrderBy(k => k, StringComparer.Ordinal);

        Assert.Equal(scores.OrderByDescending(s => s).Select(s => LeaderboardRanking.RankedRowKey(s, "x")), sorted);
        Assert.Equal(11, LeaderboardRanking.RankedRowKey(int.MaxValue, "x").IndexOf('_'));
        Assert.True(LeaderboardRanking.IsRanked("R"));
    }

    [Fact]
    public void RankedRowKey_FallsInRankedRange()
    {
        foreach (var score in new[] { int.MinValue, -1, 0, 1, 999_999, int.MaxValue })
            Assert.True(LeaderboardRanking.IsRanked(LeaderboardRanking.RankedRowKey(score, Guid.NewGuid().ToString())));
    }

    [Theory]
    [InlineData("Current")]
    [InlineData("S")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("9fffffff-ffff-ffff-ffff-ffffffffffff")]
    [InlineData("a0000000-0000-0000-0000-000000000000")]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public void LegacyKey_FallsOutsideRankedRange(string rowKey) =>
        Assert.False(LeaderboardRanking.IsRanked(rowKey));

    private static async IAsyncEnumerable<int> Counting(int count, Action onYield)
    {
        for (var i = 0; i < count; i++)
        {
            onYield();
            yield return i;
            await Task.Yield();
        }
    }

    [Fact]
    public async Task TakeTopAsync_LongSource_ReturnsTenAndStopsEnumerating()
    {
        var yielded = 0;
        var top = await LeaderboardRanking.TakeTopAsync(Counting(25, () => yielded++));

        Assert.Equal(10, top.Count);
        Assert.Equal(10, yielded);
        Assert.Equal(3, (await LeaderboardRanking.TakeTopAsync(Counting(3, () => { }))).Count);
        Assert.Empty(await LeaderboardRanking.TakeTopAsync(Counting(0, () => { })));
    }

    private static StoredScore Legacy(string key, int score) => new(P, key, "L" + score, score);

    private static StoredScore Ranked(string id, int score) =>
        new(P, LeaderboardRanking.RankedRowKey(score, id), "R" + score, score);

    [Fact]
    public void Merge_LegacyOnly_KeepsTopTen()
    {
        var rows = Enumerable.Range(1, 12).Select(i => Legacy($"{i:D2}-guid", i * 10)).ToList();

        var top = LeaderboardRanking.Merge(rows, []);

        Assert.Equal(10, top.Count);
        Assert.DoesNotContain(top, r => r.Score is 10 or 20);
        Assert.Equal(rows[11], top[0]);
    }

    [Fact]
    public void Merge_MatchesTodaysSort()
    {
        var legacy = new[]
        {
            Legacy("Current", 50), Legacy("0a-guid", 70), Legacy("3b-guid", 70), Legacy("9c-guid", 20),
            Legacy("a1-guid", 70), Legacy("b2-guid", 50), Legacy("c3-guid", 90), Legacy("d4-guid", 5),
            Legacy("e5-guid", 50), Legacy("f6-guid", 70)
        };
        var ranked = new[]
        {
            Ranked("1d-guid", 70), Ranked("4e-guid", 50), Ranked("a9-guid", 90), Ranked("b8-guid", 50),
            Ranked("c7-guid", 1), Ranked("d6-guid", 70), Ranked("e4-guid", 20), Ranked("f3-guid", 100),
            Ranked("2f-guid", 50), Ranked("5a-guid", 60)
        };

        // Today: every row has the plain id as RowKey; rows arrive in RowKey order, sorted stably by score
        string Id(StoredScore r) => LeaderboardRanking.IsRanked(r.RowKey) ? r.RowKey[(r.RowKey.IndexOf('_') + 1)..] : r.RowKey;
        var expected = legacy.Concat(ranked)
            .OrderBy(Id, StringComparer.Ordinal)
            .OrderByDescending(r => r.Score)
            .Take(10)
            .ToList();

        Assert.Equal(expected, LeaderboardRanking.Merge(legacy, ranked));
        Assert.Equal(5, LeaderboardRanking.Merge(legacy.Take(2), ranked.Take(3)).Count);
    }

    [Fact]
    public void TryReadScore_TypeCases()
    {
        Assert.True(LeaderboardRanking.TryReadScore(null, out var none));
        Assert.Equal(0, none);
        Assert.True(LeaderboardRanking.TryReadScore(42, out var value));
        Assert.Equal(42, value);
        Assert.False(LeaderboardRanking.TryReadScore(42L, out _));
        Assert.False(LeaderboardRanking.TryReadScore(4.2, out _));
        Assert.False(LeaderboardRanking.TryReadScore("42", out _));
    }
}
