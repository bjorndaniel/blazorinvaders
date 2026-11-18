using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BlazorInvaders.Api;

/// <param name="Mode">"daily" for a daily challenge score, otherwise null (all-time board).</param>
/// <param name="Day">The challenge's UTC date as yyyyMMdd, for daily scores.</param>
/// <param name="Wave">The wave the game reached; required, and bounds the plausible score.</param>
public record HighScoreRequest(string Name, int Score, string? Mode = null, string? Day = null, int Wave = 0);

public static partial class HighScoreValidator
{
    public const int MaxBodyBytes = 1024;
    public const int MaxScore = 999_999;
    public const string DailyMode = "daily";
    public const string AllTimePartition = "HighScore";

    // The scoring below must match GameEngine.cs; ScorePlausibilityTests fails if it drifts.
    // Aliens: the 11x5 grid is worth 990 points at 1x, and the combo multiplier peaks at 4x.
    internal const int GridPoints = 990;
    internal const int MaxComboMultiplier = 4;
    // Mothership: the best score in its table. A wave allows at most 200 hits: a pass crosses in
    // about 18 ticks, one is on screen at a time, and a hit clears it 2 ticks later, ~175 at most.
    internal const int MotherShipMaxPoints = 300;
    internal const int MaxMotherShipsPerWave = 200;
    internal const int PerfectWaveBonus = 500;
    // Boss: a hit is worth 50 (combo-multiplied), its HP is 30 + 10 per earlier boss, and the kill bonus is 1000 per boss
    internal const int BossHitPoints = 50;
    internal const int BossKillBonus = 1000;
    internal const int BossBaseHp = 30;
    internal const int BossHpPerBoss = 10;
    // A boss every 5 waves, or every 3 with the daily Boss Rush modifier
    internal const int BossEvery = 5;
    internal const int BossRushEvery = 3;
    // The daily Glass modifier doubles every award
    internal const int DailyScoreFactor = 2;

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

    private const long AlienWaveMax =
        (long)GridPoints * MaxComboMultiplier + (long)MaxMotherShipsPerWave * MotherShipMaxPoints + PerfectWaveBonus;

    /// <summary>The most a boss wave can score: the boss's hits and kill bonus, plus the perfect-wave bonus. For tests.</summary>
    internal static long BossWaveMax(int boss) =>
        (long)BossHitPoints * MaxComboMultiplier * (BossBaseHp + BossHpPerBoss * (boss - 1)) +
        (long)BossKillBonus * boss + PerfectWaveBonus;

    /// <summary>
    /// The most a game could score by <paramref name="wave"/>: each wave's best case, doubled for
    /// daily games (the modifier is not sent, so Glass is assumed). Stops once it passes
    /// <see cref="MaxScore"/>.
    /// </summary>
    internal static long MaxPlausibleScore(int wave, bool daily)
    {
        long total = 0;
        for (var w = 1; w <= wave && total <= MaxScore; w++)
        {
            var best = AlienWaveMax;
            var boss = 0;
            if (w % BossEvery == 0)
                boss = w / BossEvery;
            // Daily games may be Boss Rush, whose bosses come sooner and are tougher
            if (daily && w % BossRushEvery == 0)
                boss = Math.Max(boss, w / BossRushEvery);
            if (boss > 0)
                best = Math.Max(best, BossWaveMax(boss));
            total += best * (daily ? DailyScoreFactor : 1);
        }
        return total;
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

        if (dto.Wave < 1)
        {
            error = "Wave reached is required";
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

        if (dto.Score % 10 != 0 || dto.Score > MaxPlausibleScore(dto.Wave, mode == DailyMode))
        {
            error = "Score is not possible for the wave reached";
            return false;
        }

        score = new HighScoreRequest(name, dto.Score, mode, day, dto.Wave);
        error = string.Empty;
        return true;
    }
}
