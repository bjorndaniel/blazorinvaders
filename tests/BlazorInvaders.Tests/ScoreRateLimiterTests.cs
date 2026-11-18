using System.Security.Cryptography;
using System.Text;
using BlazorInvaders.Api;
using Xunit;

namespace BlazorInvaders.Tests;

public class ScoreRateLimiterTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);
    private static readonly byte[] Salt = Encoding.UTF8.GetBytes("test-salt-0123456789-test-salt-0");

    private static ScoreRateLimiter NewLimiter(int maxTracked = 10_000, byte[]? salt = null) =>
        new(TimeProvider.System, salt ?? Salt, maxTracked);

    private static void Post(ScoreRateLimiter limiter, string address, DateTimeOffset at, int times)
    {
        for (var i = 0; i < times; i++)
            Assert.True(limiter.TryAcquire(address, at, out _, out _));
    }

    [Fact]
    public void TryAcquire_PastLimit_RejectsWithRetryAfter()
    {
        using var limiter = NewLimiter();
        Post(limiter, "1.2.3.4", T0, ScoreRateLimiter.MaxPostsPerWindow);

        Assert.False(limiter.TryAcquire("1.2.3.4", T0.AddMinutes(10), out var retryAfter, out _));
        Assert.Equal(TimeSpan.FromMinutes(50), retryAfter);
    }

    [Fact]
    public void TryAcquire_WindowEnds_AllowsAgain()
    {
        using var limiter = NewLimiter();
        Post(limiter, "1.2.3.4", T0, ScoreRateLimiter.MaxPostsPerWindow);

        Assert.False(limiter.TryAcquire("1.2.3.4", T0 + ScoreRateLimiter.Window - TimeSpan.FromTicks(1), out _, out _));
        Assert.True(limiter.TryAcquire("1.2.3.4", T0 + ScoreRateLimiter.Window, out _, out _));
    }

    [Fact]
    public void TryAcquire_OtherClient_IsIndependent()
    {
        using var limiter = NewLimiter();
        Post(limiter, "1.2.3.4", T0, ScoreRateLimiter.MaxPostsPerWindow);

        Assert.True(limiter.TryAcquire("5.6.7.8", T0, out _, out _));
    }

    [Fact]
    public void TryAcquire_TableFull_UsesSharedOverflowBucket()
    {
        using var limiter = NewLimiter(maxTracked: 3);
        for (var i = 0; i < 3; i++)
            Post(limiter, $"10.0.0.{i}", T0, 1);

        for (var i = 0; i < ScoreRateLimiter.MaxPostsPerWindow; i++)
            Assert.True(limiter.TryAcquire($"10.0.1.{i}", T0, out _, out _));
        Assert.False(limiter.TryAcquire("10.0.2.1", T0, out _, out _));
        Assert.True(limiter.TrackedClients <= 4);
    }

    [Fact]
    public void TryAcquire_TableFullOfEndedWindows_PrunesInline()
    {
        using var limiter = NewLimiter(maxTracked: 3);
        for (var i = 0; i < 3; i++)
            Post(limiter, $"10.0.0.{i}", T0, 1);

        Post(limiter, "10.0.1.1", T0 + ScoreRateLimiter.Window, 1);

        Assert.Equal(1, limiter.TrackedClients);
    }

    [Fact]
    public void ClientKey_IsSaltedHash()
    {
        using var limiter = NewLimiter();
        var key = limiter.ClientKey("1.2.3.4");

        Assert.DoesNotContain("1.2.3.4", key);
        Assert.NotEqual(Convert.ToHexString(SHA256.HashData([1, 2, 3, 4])), key);
        Assert.Equal(key, limiter.ClientKey("1.2.3.4"));
        using var other = NewLimiter(salt: Encoding.UTF8.GetBytes("another-salt-0123456789-another-"));
        Assert.NotEqual(key, other.ClientKey("1.2.3.4"));
    }

    [Fact]
    public void ClientKey_Normalizes()
    {
        using var limiter = NewLimiter();

        Assert.Equal(limiter.ClientKey("1.2.3.4"), limiter.ClientKey("1.2.3.4:5678"));
        Assert.Equal(limiter.ClientKey("2001:db8:1:2::a"), limiter.ClientKey("2001:db8:1:2::b"));
        Assert.Equal(limiter.ClientKey("2001:db8:1:2::a"), limiter.ClientKey("[2001:db8:1:2::a]:80"));
        Assert.NotEqual(limiter.ClientKey("2001:db8:1:2::a"), limiter.ClientKey("2001:db8:1:3::a"));
        // Same /64, differing in bytes 8-11
        Assert.Equal(limiter.ClientKey("2001:db8:1:2::a"), limiter.ClientKey("2001:db8:1:2:ffff:ffff::a"));
        Assert.Equal(limiter.ClientKey("1.2.3.4"), limiter.ClientKey("::ffff:1.2.3.4"));
        Assert.Equal(limiter.ClientKey("garbage"), limiter.ClientKey(null));
        Assert.Equal(limiter.ClientKey("garbage"), limiter.ClientKey(""));
        Assert.NotEqual(limiter.ClientKey("garbage"), limiter.ClientKey("1.2.3.4"));
    }

    [Fact]
    public void ClientAddress_PrefersXClientIp_ElseRightmostForwardedFor()
    {
        Assert.Equal("1.1.1.1", ScoreRateLimiter.ClientAddress("1.1.1.1", "9.9.9.9, 2.2.2.2"));
        Assert.Equal("2.2.2.2", ScoreRateLimiter.ClientAddress(null, "9.9.9.9, 2.2.2.2"));
        Assert.Equal("2.2.2.2", ScoreRateLimiter.ClientAddress(" ", "2.2.2.2"));
        Assert.Null(ScoreRateLimiter.ClientAddress(null, null));
        Assert.Null(ScoreRateLimiter.ClientAddress("", " "));
    }

    [Fact]
    public void Prune_RemovesEndedWindowsOnly()
    {
        using var limiter = NewLimiter();
        Post(limiter, "1.1.1.1", T0, 1);
        Post(limiter, "2.2.2.2", T0.AddMinutes(30), 1);

        limiter.Prune(T0 + ScoreRateLimiter.Window);
        Assert.Equal(1, limiter.TrackedClients);

        limiter.Prune(T0.AddMinutes(90));
        Assert.Equal(0, limiter.TrackedClients);
    }

    private sealed class TimerTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = T0;
        public TimerCallback? Callback;
        public TimeSpan DueTime, Period;
        public override DateTimeOffset GetUtcNow() => Now;
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            Callback = callback;
            DueTime = dueTime;
            Period = period;
            return new NoopTimer();
        }
    }

    private sealed class NoopTimer : ITimer
    {
        public bool Change(TimeSpan dueTime, TimeSpan period) => true;
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    [Fact]
    public void Timer_DropsEndedWindows_WithoutTraffic()
    {
        var time = new TimerTime();
        using var limiter = new ScoreRateLimiter(time, Salt, 10_000);
        Assert.Equal(ScoreRateLimiter.Window, time.DueTime);
        Assert.Equal(ScoreRateLimiter.Window, time.Period);
        Post(limiter, "1.2.3.4", T0, 1);
        Assert.Equal(1, limiter.TrackedClients);

        time.Now = T0 + ScoreRateLimiter.Window + TimeSpan.FromSeconds(1);
        time.Callback!(null);

        Assert.Equal(0, limiter.TrackedClients);
    }

    [Fact]
    public void TryAcquire_FullTableScan_IsThrottledToOncePerMinute()
    {
        using var limiter = NewLimiter(maxTracked: 3);
        // The first full-table event prunes (nothing ended yet) and starts the one-minute throttle
        for (var i = 0; i < 3; i++)
            Post(limiter, $"10.0.0.{i}", T0, 1);
        Post(limiter, "10.0.1.1", T0, 1);
        Assert.Equal(4, limiter.TrackedClients);

        // 59m30s later: the first three windows have not ended; a scan would not remove them anyway
        Post(limiter, "10.0.1.2", T0.AddMinutes(59).AddSeconds(30), 1);
        // At T0+60m the windows have ended, but the last scan (T0+59m30s) was under a minute ago
        Post(limiter, "10.0.1.3", T0.AddMinutes(60), 1);
        Assert.Equal(4, limiter.TrackedClients);
    }

    [Fact]
    public void TryAcquire_ReportsBucket()
    {
        using var limiter = NewLimiter(maxTracked: 1);
        Assert.True(limiter.TryAcquire("1.2.3.4", T0, out _, out var bucket));
        Assert.Equal(ScoreRateLimiter.RateBucket.Client, bucket);
        Assert.True(limiter.TryAcquire("5.6.7.8", T0, out _, out bucket));
        Assert.Equal(ScoreRateLimiter.RateBucket.Overflow, bucket);
        using var other = NewLimiter();
        Assert.True(other.TryAcquire(null, T0, out _, out bucket));
        Assert.Equal(ScoreRateLimiter.RateBucket.Unknown, bucket);
    }
}
