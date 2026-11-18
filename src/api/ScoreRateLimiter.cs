using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace BlazorInvaders.Api;

/// <summary>
/// Counts score posts per client in a fixed window and says when a client has posted too many.
/// Clients are keyed on a salted HMAC of their IP address, so the address itself is never held.
/// Everything lives in memory, per instance: the salt is random per process, and a timer drops
/// counters whose window has ended, so a record lives at most twice <see cref="Window"/>.
/// </summary>
public sealed class ScoreRateLimiter : IDisposable
{
    /// <summary>The counter a post was counted against: the client's own, the unknown-address one, or the overflow one.</summary>
    public enum RateBucket { Client, Unknown, Overflow }

    /// <summary>Covers the 10 queued scores a client sends after being offline, plus the current game.</summary>
    public const int MaxPostsPerWindow = 20;

    public static readonly TimeSpan Window = TimeSpan.FromHours(1);

    /// <summary>Bounds memory (about 1 MB); further clients share one limited overflow bucket.</summary>
    public const int DefaultMaxTrackedClients = 10_000;

    /// <summary>A full table is scanned inline at most this often, so keeping it full costs one scan a minute.</summary>
    private static readonly TimeSpan InlinePruneInterval = TimeSpan.FromMinutes(1);

    // Real keys are 64 hex characters, so these cannot collide with one
    private const string UnknownKey = "unknown";
    private const string OverflowKey = "overflow";

    private sealed class Counter(DateTimeOffset start)
    {
        public DateTimeOffset Start { get; } = start;
        public int Count { get; set; }
    }

    private readonly byte[] _salt;
    private readonly int _maxTrackedClients;
    private readonly Dictionary<string, Counter> _counters = new();
    private readonly object _lock = new();
    private readonly ITimer _timer;
    // Starts at MinValue so the first full-table event always prunes, whatever clock the caller uses
    private DateTimeOffset _lastPrune = DateTimeOffset.MinValue;

    public ScoreRateLimiter(TimeProvider time)
        : this(time, RandomNumberGenerator.GetBytes(32), DefaultMaxTrackedClients)
    {
    }

    /// <summary>For tests: a known salt and table size. DI only sees the public constructor.</summary>
    internal ScoreRateLimiter(TimeProvider time, byte[] salt, int maxTrackedClients)
    {
        _salt = salt;
        _maxTrackedClients = maxTrackedClients;
        // Expiry must not depend on POST traffic: with no posts, the records still go
        _timer = time.CreateTimer(_ => Prune(time.GetUtcNow()), null, Window, Window);
    }

    /// <summary>Clients and special buckets held. For tests.</summary>
    internal int TrackedClients
    {
        get { lock (_lock) return _counters.Count; }
    }

    public void Dispose() => _timer.Dispose();

    /// <summary>
    /// The caller's address: <c>X-Client-IP</c> (set by the App Service front end), else the
    /// rightmost <c>X-Forwarded-For</c> entry, which the front end appends. Null if neither exists.
    /// </summary>
    public static string? ClientAddress(string? xClientIp, string? xForwardedFor)
    {
        if (!string.IsNullOrWhiteSpace(xClientIp))
            return xClientIp.Trim();

        if (string.IsNullOrWhiteSpace(xForwardedFor))
            return null;

        var last = xForwardedFor.Split(',')[^1].Trim();
        return last.Length == 0 ? null : last;
    }

    /// <summary>
    /// The salted hash a client is counted under. Strips a port, maps IPv4-mapped IPv6 to IPv4 and
    /// masks IPv6 to its /64. A missing or unparsable address shares one bucket.
    /// </summary>
    internal string ClientKey(string? address)
    {
        if (!TryParseAddress(address, out var ip))
            return UnknownKey;

        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        var bytes = ip.GetAddressBytes();
        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            Array.Clear(bytes, 8, 8);

        return Convert.ToHexString(HMACSHA256.HashData(_salt, bytes));
    }

    private static bool TryParseAddress(string? address, out IPAddress ip)
    {
        ip = IPAddress.None;
        if (string.IsNullOrWhiteSpace(address))
            return false;

        var text = address.Trim();
        if (text.StartsWith('['))
        {
            var close = text.IndexOf(']');
            if (close < 0)
                return false;
            text = text[1..close];
        }
        else if (text.Count(c => c == ':') == 1)
        {
            // IPv4 with a port
            text = text[..text.IndexOf(':')];
        }

        if (!IPAddress.TryParse(text, out var parsed))
            return false;

        ip = parsed;
        return true;
    }

    /// <summary>Drops every counter whose window has ended.</summary>
    internal void Prune(DateTimeOffset now)
    {
        lock (_lock)
            PruneLocked(now);
    }

    private void PruneLocked(DateTimeOffset now)
    {
        foreach (var key in _counters.Where(c => now >= c.Value.Start + Window).Select(c => c.Key).ToList())
            _counters.Remove(key);
        _lastPrune = now;
    }

    /// <summary>
    /// Counts one post. Returns false when the client is past the limit, with how long until its
    /// window ends in <paramref name="retryAfter"/>.
    /// </summary>
    public bool TryAcquire(string? address, DateTimeOffset now, out TimeSpan retryAfter, out RateBucket bucket)
    {
        var key = ClientKey(address);
        bucket = key == UnknownKey ? RateBucket.Unknown : RateBucket.Client;
        lock (_lock)
        {
            if (!_counters.TryGetValue(key, out var counter) && _counters.Count >= _maxTrackedClients)
            {
                if (now >= _lastPrune + InlinePruneInterval)
                    PruneLocked(now);
                // Still full: count against the shared bucket so the limit stays on
                if (_counters.Count >= _maxTrackedClients)
                {
                    key = OverflowKey;
                    bucket = RateBucket.Overflow;
                    _counters.TryGetValue(key, out counter);
                }
            }

            if (counter is not null && now >= counter.Start + Window)
                counter = null;

            if (counter is null)
            {
                counter = new Counter(now);
                _counters[key] = counter;
            }

            if (counter.Count >= MaxPostsPerWindow)
            {
                retryAfter = counter.Start + Window - now;
                return false;
            }

            counter.Count++;
            retryAfter = TimeSpan.Zero;
            return true;
        }
    }
}
