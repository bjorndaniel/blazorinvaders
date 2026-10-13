using System.Text.Json.Serialization;

namespace BlazorInvaders.GameObjects
{
    public class HighScore
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

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
    }
}