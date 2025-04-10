using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SlackBot.Clients.Interfaces;
using SlackBot.ResponseModel.SlackApi;

namespace SlackBot
{
    public class SlackBot
    {
        private readonly ILogger _logger;
        private readonly ISlackClient _slackClient;

        public SlackBot(ILoggerFactory loggerFactory, ISlackClient slackClient)
        {
            _logger = loggerFactory.CreateLogger<SlackBot>();
            _slackClient = slackClient;
        }

        [Function("DiarySummury")]
        public async Task DiarySummury([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            try
            {
                // 1. Get all channels the bot is a member of
                _logger.LogInformation("Getting bot channels...");
                List<Channel> channels = await _slackClient.GetBotChannels();
                _logger.LogInformation($"Found {channels.Count} channels");

                // Process each channel
                foreach (var channel in channels)
                {
                    if (channel.Id == null)
                    {
                        _logger.LogWarning("Skipping channel with null ID");
                        continue;
                    }

                    _logger.LogInformation($"Processing channel: {channel.Name} (ID: {channel.Id})");

                    // 2. Get today's messages from the channel
                    var today = DateTime.Today;
                    var messages = await _slackClient.GetChannelHistory(channel.Id, today);
                    _logger.LogInformation($"Retrieved {messages.Count} messages from channel {channel.Name} for {today:yyyy-MM-dd}");

                    if (messages.Count > 0)
                    {
                        // 3. Process messages (for example, create a summary)
                        string summary = CreateMessageSummary(messages);
                        
                        // 4. Send the summary back to the channel
                        bool sent = await _slackClient.SendMessageToChannel(channel.Id, summary);
                        if (sent)
                        {
                            _logger.LogInformation($"Successfully sent summary to channel {channel.Name}");
                        }
                        else
                        {
                            _logger.LogError($"Failed to send summary to channel {channel.Name}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"No messages found in channel {channel.Name} for {today:yyyy-MM-dd}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Slack channels and messages");
            }
        }

        /// <summary>
        /// Creates a summary of messages
        /// </summary>
        /// <param name="messages">The messages to summarize</param>
        /// <returns>A summary message</returns>
        private string CreateMessageSummary(List<Message> messages)
        {
            // This is a simple example - you could implement more sophisticated summarization logic
            int messageCount = messages.Count;
            int userCount = messages.Select(m => m.User).Distinct().Count();
            
            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"*Daily Summary*");
            summary.AppendLine($"Total messages today: {messageCount}");
            summary.AppendLine($"Active users: {userCount}");
            summary.AppendLine();
            
            // Add a few recent messages as examples
            var recentMessages = messages.OrderByDescending(m => m.Ts).Take(3);
            if (recentMessages.Any())
            {
                summary.AppendLine("*Recent messages:*");
                foreach (var message in recentMessages)
                {
                    var timestamp = message.Ts != null 
                        ? UnixTimestampToDateTime(message.Ts).ToString("HH:mm:ss")
                        : "unknown time";
                    
                    summary.AppendLine($"• [{timestamp}] <@{message.User}>: {message.Text}");
                }
            }
            
            return summary.ToString();
        }

        /// <summary>
        /// Converts a Unix timestamp to a DateTime
        /// </summary>
        private static DateTime UnixTimestampToDateTime(string timestamp)
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
