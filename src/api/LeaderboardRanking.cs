using System.Globalization;

namespace BlazorInvaders.Api;

/// <summary>
/// Ranked RowKeys: Table Storage cannot sort by a property, so a new score's RowKey is
/// "R" + (int.MaxValue - score) as 10 digits + "_" + id. RowKey order is then score descending.
/// Legacy rows (GUID RowKeys, "Current") all fall outside the range ["R", "S").
/// </summary>
public static class LeaderboardRanking
{
    public const int MaxEntries = 10;
    public const string RangeStart = "R";
    public const string RangeEnd = "S";

    public static string RankedRowKey(int score, string id) =>
        string.Create(CultureInfo.InvariantCulture, $"{RangeStart}{(long)int.MaxValue - score:D10}_{id}");

    public static bool IsRanked(string rowKey) =>
        string.CompareOrdinal(rowKey, RangeStart) >= 0 && string.CompareOrdinal(rowKey, RangeEnd) < 0;

    /// <summary>Ordinal order of this key is the board order: score descending, then RowKey ascending.</summary>
    public static string SortKey(StoredScore row) =>
        IsRanked(row.RowKey) ? row.RowKey : RankedRowKey(row.Score, row.RowKey);

    /// <summary>A missing Score reads as 0; any type other than Int32 is unreadable.</summary>
    public static bool TryReadScore(object? raw, out int score)
    {
        switch (raw)
        {
            case null:
                score = 0;
                return true;
            case int value:
                score = value;
                return true;
            default:
                score = 0;
                return false;
        }
    }

    /// <summary>The best <see cref="MaxEntries"/> rows of both lists, in board order.</summary>
    public static IReadOnlyList<StoredScore> Merge(IEnumerable<StoredScore> a, IEnumerable<StoredScore> b) =>
        a.Concat(b)
            .OrderBy(SortKey, StringComparer.Ordinal)
            .Take(MaxEntries)
            .ToList();

    /// <summary>Reads at most <see cref="MaxEntries"/> items, then stops enumerating.</summary>
    public static async Task<IReadOnlyList<T>> TakeTopAsync<T>(IAsyncEnumerable<T> source, CancellationToken ct = default)
    {
        var items = new List<T>(MaxEntries);
        await foreach (var item in source.WithCancellation(ct))
        {
            items.Add(item);
            if (items.Count >= MaxEntries)
                break;
        }
        return items;
    }
}
