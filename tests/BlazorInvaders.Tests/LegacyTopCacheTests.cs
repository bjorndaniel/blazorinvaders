using BlazorInvaders.Api;
using Xunit;

namespace BlazorInvaders.Tests;

public class LegacyTopCacheTests
{
    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 11, 17, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static readonly IReadOnlyList<StoredScore> OneRow = [new("p", "k", "AAA", 5)];

    private static LegacyTopCache Cache(
        Func<string, Task<IReadOnlyList<StoredScore>?>> load, TimeProvider time, Action<string, Exception>? onFail = null) =>
        new((p, _) => load(p), time, onFail);

    [Fact]
    public async Task NonEmpty_LoadedOncePerPartition()
    {
        var loads = 0;
        var cache = Cache(_ => { loads++; return Task.FromResult<IReadOnlyList<StoredScore>?>(OneRow); }, new ManualTime());

        for (var i = 0; i < 3; i++)
            Assert.Equal(OneRow, await cache.GetAsync("HighScore", default));
        Assert.Equal(1, loads);

        await cache.GetAsync("Daily-20261117", default);
        Assert.Equal(2, loads);
        Assert.Equal(2, cache.CachedCount);
    }

    [Fact]
    public async Task Empty_NotCached()
    {
        var loads = 0;
        var cache = Cache(_ => { loads++; return Task.FromResult<IReadOnlyList<StoredScore>?>([]); }, new ManualTime());

        for (var i = 0; i < 100; i++)
            Assert.Empty(await cache.GetAsync($"Daily-{i}", default));

        Assert.Equal(100, loads);
        Assert.Equal(0, cache.CachedCount);
    }

    [Fact]
    public async Task Failure_BacksOffFiveMinutes()
    {
        var time = new ManualTime();
        var loads = 0;
        var fail = false;
        var cache = Cache(p =>
        {
            loads++;
            return Task.FromResult<IReadOnlyList<StoredScore>?>(fail ? null : OneRow);
        }, time);

        await cache.GetAsync("HighScore", default);
        fail = true;
        Assert.Empty(await cache.GetAsync("Daily-1", default));
        Assert.Equal(2, loads);

        time.Now += TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(59);
        Assert.Empty(await cache.GetAsync("Daily-1", default));
        Assert.Equal(2, loads);
        Assert.Equal(OneRow, await cache.GetAsync("HighScore", default));

        time.Now += TimeSpan.FromSeconds(1);
        await cache.GetAsync("Daily-1", default);
        Assert.Equal(3, loads);
    }

    [Fact]
    public async Task FaultedLoad_CountsAsFailed_AndIsNotKept()
    {
        var time = new ManualTime();
        var loads = 0;
        var reported = 0;
        var cache = Cache(_ => { loads++; throw new InvalidOperationException("bad row"); }, time, (_, _) => reported++);

        Assert.Empty(await cache.GetAsync("HighScore", default));
        Assert.Equal(1, reported);
        Assert.Equal(0, cache.CachedCount);

        Assert.Empty(await cache.GetAsync("HighScore", default));
        Assert.Equal(1, loads);

        time.Now += LegacyTopCache.RetryInterval;
        await cache.GetAsync("HighScore", default);
        Assert.Equal(2, loads);
    }

    [Fact]
    public async Task ConcurrentFirstCalls_ShareOneLoad()
    {
        var loads = 0;
        var gate = new TaskCompletionSource<IReadOnlyList<StoredScore>?>();
        var cache = Cache(_ => { loads++; return gate.Task; }, new ManualTime());

        var first = cache.GetAsync("HighScore", default);
        var second = cache.GetAsync("HighScore", default);
        gate.SetResult(OneRow);

        Assert.Equal(OneRow, await first);
        Assert.Equal(OneRow, await second);
        Assert.Equal(1, loads);
    }
}
