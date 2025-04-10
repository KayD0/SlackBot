using SlackBot.Clients.Interfaces;
using SlackBot.ResponseModel.SlackApi;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SlackBot.Clients
{
    /// <summary>
    /// Service for interacting with the Slack API
    /// </summary>
    public class SlackClient : ISlackClient
    {
        private readonly string _slackBotToken;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the SlackService class
        /// </summary>
        /// <param name="slackBotToken">The Slack bot token to use for API calls</param>
        /// <param name="httpClient">The HttpClient to use for API calls (optional)</param>
        public SlackClient(string slackBotToken, HttpClient? httpClient = null)
        {
            _slackBotToken = slackBotToken ?? throw new ArgumentNullException(nameof(slackBotToken));
            _httpClient = httpClient ?? new HttpClient();

            // Set the Authorization header if it's not already set
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _slackBotToken);
            }
        }

        /// <summary>
        /// Retrieves all channels that the bot is a member of
        /// </summary>
        /// <returns>List of channels</returns>
        public async Task<List<Channel>> GetBotChannels()
        {
            // Call Slack API to get conversations list
            var response = await _httpClient.GetAsync("https://slack.com/api/conversations.list?types=public_channel,private_channel&limit=1000");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var conversationsResponse = JsonSerializer.Deserialize<ConversationsListResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (conversationsResponse == null)
            {
                throw new Exception("Failed to deserialize Slack API response");
            }

            if (!conversationsResponse.Ok)
            {
                throw new Exception($"Slack API error: {conversationsResponse.Error}");
            }

            // Filter to only include channels the bot is a member of
            var botChannels = new List<Channel>();

            if (conversationsResponse.Channels != null)
            {
                foreach (var channel in conversationsResponse.Channels)
                {
                    // Check if the bot is a member of this channel
                    if (channel.IsMember)
                    {
                        botChannels.Add(channel);
                    }
                }
            }

            return botChannels;
        }

        /// <summary>
        /// Retrieves chat history for a specific channel on a specific date
        /// </summary>
        /// <param name="channelId">The ID of the channel</param>
        /// <param name="date">The date to retrieve history for</param>
        /// <returns>List of messages</returns>
        public async Task<List<Message>> GetChannelHistory(string channelId, DateTime date)
        {
            // Calculate Unix timestamps for the start and end of the day
            DateTime startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local);
            DateTime endOfDay = startOfDay.AddDays(1).AddSeconds(-1);

            double oldest = DateTimeToUnixTimestamp(startOfDay);
            double latest = DateTimeToUnixTimestamp(endOfDay);

            // Call Slack API to get conversation history
            string url = $"https://slack.com/api/conversations.history?channel={channelId}&oldest={oldest}&latest={latest}&limit=1000";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var historyResponse = JsonSerializer.Deserialize<ConversationHistoryResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (historyResponse == null)
            {
                throw new Exception("Failed to deserialize Slack API response");
            }

            if (!historyResponse.Ok)
            {
                throw new Exception($"Slack API error: {historyResponse.Error}");
            }

            return historyResponse.Messages ?? new List<Message>();
        }

        /// <summary>
        /// Retrieves a list of all users in the Slack workspace
        /// </summary>
        /// <returns>List of Slack users</returns>
        public async Task<List<SlackUser>> GetUserList()
        {
            // Call Slack API to get users list
            var response = await _httpClient.GetAsync("https://slack.com/api/users.list");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var usersResponse = JsonSerializer.Deserialize<UsersListResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (usersResponse == null)
            {
                throw new Exception("Failed to deserialize Slack API response");
            }

            if (!usersResponse.Ok)
            {
                throw new Exception($"Slack API error: {usersResponse.Error}");
            }

            return usersResponse.Members ?? new List<SlackUser>();
        }

        /// <summary>
        /// Sends a message to a specified channel
        /// </summary>
        /// <param name="channelId">The ID of the channel to send the message to</param>
        /// <param name="message">The message text to send</param>
        /// <returns>True if the message was sent successfully, false otherwise</returns>
        public async Task<bool> SendMessageToChannel(string channelId, string message)
        {
            try
            {
                // Create the request content
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("channel", channelId),
                    new KeyValuePair<string, string>("text", message)
                });

                // Call Slack API to send message
                var response = await _httpClient.PostAsync("https://slack.com/api/chat.postMessage", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var postMessageResponse = JsonSerializer.Deserialize<SlackApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (postMessageResponse == null)
                {
                    Console.WriteLine("Failed to deserialize Slack API response");
                    return false;
                }

                if (!postMessageResponse.Ok)
                {
                    Console.WriteLine($"Slack API error: {postMessageResponse.Error}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts a DateTime to a Unix timestamp (seconds since epoch)
        /// </summary>
        private static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (dateTime.ToUniversalTime() - epoch).TotalSeconds;
        }

        /// <summary>
        /// Converts a Unix timestamp to a DateTime
        /// </summary>
        public static DateTime UnixTimestampToDateTime(string timestamp)
        {
            if (double.TryParse(timestamp, out double unixTimeStamp))
            {
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(unixTimeStamp).ToLocalTime();
            }

            return DateTime.MinValue;
        }
    }
}
