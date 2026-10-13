using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BlazorInvaders.Api;

/// <param name="Mode">"daily" for a daily challenge score, otherwise null (all-time board).</param>
/// <param name="Day">The challenge's UTC date as yyyyMMdd, for daily scores.</param>
public record HighScoreRequest(string Name, int Score, string? Mode = null, string? Day = null);

public static partial class HighScoreValidator
{
    public const int MaxBodyBytes = 1024;
    public const int MaxScore = 999_999;
    public const string DailyMode = "daily";
    public const string AllTimePartition = "HighScore";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [GeneratedRegex("^[A-Z0-9]{1,3}$")]
    private static partial Regex InitialsPattern();

    [GeneratedRegex("^[0-9]{8}$")]
    private static partial Regex DayPattern();

    private static DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>The table partition a validated score is stored in.</summary>
    public static string PartitionFor(HighScoreRequest score) =>
        score.Mode == DailyMode ? DailyPartition(score.Day!) : AllTimePartition;

    private static string DailyPartition(string day) => $"Daily-{day}";

    /// <summary>
    /// Resolves the leaderboard partition for a query: all-time without a mode, or a day's
    /// challenge for mode "daily" (the day defaults to <paramref name="today"/>).
    /// </summary>
    public static bool TryGetLeaderboardPartition(string? mode, string? day, DateOnly today, out string partition, out string error)
    {
        partition = AllTimePartition;
        error = string.Empty;
        if (string.IsNullOrEmpty(mode))
            return true;

        if (mode != DailyMode)
        {
            error = "Unknown mode";
            return false;
        }

        day ??= today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        if (!TryParseDay(day, out _))
        {
            error = "Day must be yyyyMMdd";
            return false;
        }

        partition = DailyPartition(day);
        return true;
    }

    public static bool TryGetLeaderboardPartition(string? mode, string? day, out string partition, out string error) =>
        TryGetLeaderboardPartition(mode, day, UtcToday, out partition, out error);

    private static bool TryParseDay(string? day, out DateOnly date)
    {
        date = default;
        return day != null && DayPattern().IsMatch(day) &&
            DateOnly.TryParseExact(day, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    /// <summary>
    /// Reads the request body, returning null if it exceeds <paramref name="maxBytes"/>.
    /// </summary>
    public static async Task<byte[]?> ReadBoundedAsync(Stream body, int maxBytes)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[512];
        int read;
        while ((read = await body.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + read > maxBytes)
                return null;
            buffer.Write(chunk, 0, read);
        }
        return buffer.ToArray();
    }

    /// <summary>
    /// Parses and validates a high score submission. On success <paramref name="score"/>
    /// holds the normalized (trimmed, upper-case) entry.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<byte> json, out HighScoreRequest score, out string error) =>
        TryParse(json, UtcToday, out score, out error);

    /// <param name="today">The current UTC date; daily scores are accepted for it and the day
    /// before, so a game that crosses midnight still counts.</param>
    public static bool TryParse(ReadOnlySpan<byte> json, DateOnly today, out HighScoreRequest score, out string error)
    {
        score = new HighScoreRequest(string.Empty, 0);

        HighScoreRequest? dto;
        try
        {
            dto = JsonSerializer.Deserialize<HighScoreRequest>(json, JsonOptions);
        }
        catch (JsonException)
        {
            error = "Invalid JSON";
            return false;
        }

        if (dto is null)
        {
            error = "Missing body";
            return false;
        }

        var name = dto.Name?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!InitialsPattern().IsMatch(name))
        {
            error = "Name must be 1-3 letters (A-Z) or digits";
            return false;
        }

        if (dto.Score is < 1 or > MaxScore)
        {
            error = $"Score must be between 1 and {MaxScore}";
            return false;
        }

        string? mode = null, day = null;
        if (!string.IsNullOrEmpty(dto.Mode))
        {
            if (dto.Mode != DailyMode)
            {
                error = "Unknown mode";
                return false;
            }
            if (!TryParseDay(dto.Day, out var date) || (date != today && date != today.AddDays(-1)))
            {
                error = "Daily scores must be for today's challenge";
                return false;
            }
            (mode, day) = (DailyMode, dto.Day);
        }

        score = new HighScoreRequest(name, dto.Score, mode, day);
        error = string.Empty;
        return true;
    }
}
