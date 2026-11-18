using System.Net;
using System.Text.Json.Serialization;

namespace BlazorInvaders.GameObjects
{
    /// <summary>One leaderboard entry, in the shape the API stores and returns.</summary>
    public class HighScore
    {
        /// <summary>The player's name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>The points scored.</summary>
        [JsonPropertyName("score")]
        public int Score { get; set; }

        /// <summary>"daily" for a daily challenge score; omitted for the all-time board.</summary>
        [JsonPropertyName("mode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Mode { get; set; }

        /// <summary>The daily challenge's date as yyyyMMdd.</summary>
        [JsonPropertyName("day")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Day { get; set; }

        /// <summary>The wave the game reached; the API needs it to check the score is possible. Omitted when 0.</summary>
        [JsonPropertyName("wave")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Wave { get; set; }

        /// <summary>
        /// Whether a score the API turned away should be queued and sent again: too many requests (429),
        /// unavailable (503), or 403. The API never returns 403, so it comes from the platform, for
        /// example an app stopped for hitting its daily quota. Other rejections would repeat.
        /// </summary>
        public static bool ShouldRetrySend(HttpStatusCode status) =>
            status is HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable or HttpStatusCode.Forbidden;
    }
}