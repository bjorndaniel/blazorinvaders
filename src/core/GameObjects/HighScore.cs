using System.Text.Json.Serialization;

namespace BlazorInvaders.GameObjects
{
    public class HighScore
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public int Score { get; set; }
    }
}