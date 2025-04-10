using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SlackBot.ResponseModel.SlackApi
{
    /// <summary>
    /// Slack conversations.list APIからのレスポンス
    /// </summary>
    public class ConversationsListResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("channels")]
        public List<Channel>? Channels { get; set; } = new List<Channel>();

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
