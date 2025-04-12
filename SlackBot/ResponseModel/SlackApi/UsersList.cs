using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SlackBot.ResponseModel.SlackApi
{
    /// <summary>
    /// Slack users.list APIからのレスポンス
    /// </summary>
    public class UsersList
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("members")]
        public List<SlackUser>? Members { get; set; } = new List<SlackUser>();

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
