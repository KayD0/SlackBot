using System.Text.Json.Serialization;

namespace SlackBot.ResponseModel.SlackApi
{
    /// <summary>
    /// Represents a Slack channel
    /// </summary>
    public class Channel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("is_member")]
        public bool IsMember { get; set; }

        [JsonPropertyName("topic")]
        public Topic? Topic { get; set; }
    }

    /// <summary>
    /// Represents a Slack channel topic
    /// </summary>
    public class Topic
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}
