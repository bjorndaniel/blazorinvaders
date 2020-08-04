using System.Text.Json.Serialization;

namespace BlazorInvaders.GameObjects
{
    public class HighScore
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }
    }
}