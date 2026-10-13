using System.Text.Json;
using System.Text.RegularExpressions;

namespace BlazorInvaders.Api;

public record HighScoreRequest(string Name, int Score);

public static partial class HighScoreValidator
{
    public const int MaxBodyBytes = 1024;
    public const int MaxScore = 999_999;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [GeneratedRegex("^[A-Z0-9]{1,3}$")]
    private static partial Regex InitialsPattern();

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
    public static bool TryParse(ReadOnlySpan<byte> json, out HighScoreRequest score, out string error)
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

        score = new HighScoreRequest(name, dto.Score);
        error = string.Empty;
        return true;
    }
}
