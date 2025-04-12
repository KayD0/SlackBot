using System.Text.Json.Serialization;

namespace SlackBot.ResponseModel.SlackApi
{
    /// <summary>
    /// Slackメッセージを表す
    /// </summary>
    public class Message
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }

        public string? UserDisplayName { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("ts")]
        public string? Ts { get; set; }
    }
}
