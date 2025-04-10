using System.Text.Json.Serialization;

namespace SlackBot.ResponseModel.SlackApi
{
    /// <summary>
    /// Represents a Slack user
    /// </summary>
    public class SlackUser
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("real_name")]
        public string? RealName { get; set; }

        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }

        [JsonPropertyName("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonPropertyName("profile")]
        public UserProfile? Profile { get; set; }
    }

    /// <summary>
    /// Represents a Slack user profile
    /// </summary>
    public class UserProfile
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("image_72")]
        public string? Image { get; set; }
    }
}
