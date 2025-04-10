using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SlackBot.ResponseModel.SlackApi
{
    /// <summary>
    /// Slack conversations.history APIからのレスポンス
    /// </summary>
    public class ConversationHistoryResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("messages")]
        public List<Message>? Messages { get; set; } = new List<Message>();

        [JsonPropertyName("has_more")]
        public bool HasMore { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
