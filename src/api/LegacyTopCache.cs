using System.Collections.Concurrent;

namespace BlazorInvaders.Api;

/// <summary>
/// Holds each board's legacy (GUID-keyed) top 10, loaded once per board and instance.
/// Only non-empty results are kept, so arbitrary boards add no memory. After a failed load
/// no board is loaded for <see cref="RetryInterval"/>; boards already cached are still served.
/// </summary>
public sealed class LegacyTopCache(
    Func<string, CancellationToken, Task<IReadOnlyList<StoredScore>?>> loader,
    TimeProvider time,
    Action<string, Exception>? onLoadFailed = null)
{
    public static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, Lazy<Task<IReadOnlyList<StoredScore>>>> _entries = new();
    private readonly object _retryLock = new();
    private DateTimeOffset _retryAfter = DateTimeOffset.MinValue;

    /// <summary>Boards held, including loads in flight. For tests.</summary>
    internal int CachedCount => _entries.Count;

    public async Task<IReadOnlyList<StoredScore>> GetAsync(string partition, CancellationToken ct)
    {
        if (!_entries.TryGetValue(partition, out var entry))
        {
            if (time.GetUtcNow() < RetryAfter)
                return [];

            Lazy<Task<IReadOnlyList<StoredScore>>>? created = null;
            created = new(() => LoadAsync(partition, created!));
            entry = _entries.GetOrAdd(partition, created);
        }

        return await entry.Value.WaitAsync(ct);
    }

    private DateTimeOffset RetryAfter
    {
        get { lock (_retryLock) return _retryAfter; }
        set { lock (_retryLock) _retryAfter = value; }
    }

    private async Task<IReadOnlyList<StoredScore>> LoadAsync(string partition, Lazy<Task<IReadOnlyList<StoredScore>>> self)
    {
        IReadOnlyList<StoredScore>? rows;
        try
        {
            // Shared by concurrent callers, so one caller cancelling must not cancel it
            rows = await loader(partition, CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Boundary of a shared background load that no single caller owns (CG21 departure, logged via onLoadFailed)
            // A faulted load counts as failed, or the board would fail on every call
            onLoadFailed?.Invoke(partition, ex);
            rows = null;
        }

        if (rows is null)
            RetryAfter = time.GetUtcNow() + RetryInterval;

        if (rows is null || rows.Count == 0)
        {
            // Only if the entry is still ours: a newer load may have replaced it
            ((ICollection<KeyValuePair<string, Lazy<Task<IReadOnlyList<StoredScore>>>>>)_entries)
                .Remove(new(partition, self));
            return [];
        }

        return rows;
    }
}
