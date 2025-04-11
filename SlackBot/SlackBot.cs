using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackBot.Clients.Interfaces;
using SlackBot.ResponseModel.SlackApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackBot
{
    public class SlackBot
    {
        private readonly ILogger _logger;
        private readonly ISlackClient _slackClient;
        private readonly IAOAIClient _aoaiClient;
        private readonly ILMClient _lmClient;
        private readonly bool _useLMStudio;

        public SlackBot(ILoggerFactory loggerFactory, ISlackClient slackClient, 
                       IAOAIClient aoaiClient, ILMClient lmClient, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<SlackBot>();
            _slackClient = slackClient;
            _aoaiClient = aoaiClient;
            _lmClient = lmClient;
            
            // 設定からどのAIサービスを使用するか決定
            _useLMStudio = bool.TryParse(configuration["UseLMStudio"], out bool useLM) && useLM;
        }

        [Function("DiarySummury")]
        public async Task DiarySummury([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            try
            {
                // 1. ボットがメンバーになっているすべてのチャンネルを取得
                _logger.LogInformation("Getting bot channels...");
                List<Channel> channels = await _slackClient.GetBotChannels();
                _logger.LogInformation($"Found {channels.Count} channels");

                // 各チャンネルを処理
                foreach (var channel in channels)
                {
                    if (channel.Id == null)
                    {
                        _logger.LogWarning("Skipping channel with null ID");
                        continue;
                    }

                    _logger.LogInformation($"Processing channel: {channel.Name} (ID: {channel.Id})");

                    // 2. チャンネルから今日のメッセージを取得
                    var today = DateTime.Today;
                    var messages = await _slackClient.GetChannelHistory(channel.Id, today);
                    _logger.LogInformation($"Retrieved {messages.Count} messages from channel {channel.Name} for {today:yyyy-MM-dd}");

                    if (messages.Count > 0)
                    {
                        // 3. メッセージを処理（例：要約を作成）
                        string summary = await CreateMessageSummary(messages);
                        
                        // 4. 要約をチャンネルに送信
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
        /// メッセージの要約を作成
        /// </summary>
        /// <param name="messages">要約するメッセージ</param>
        /// <returns>要約メッセージ</returns>
        private async Task<string> CreateMessageSummary(List<Message> messages)
        {
            try
            {
                // AIを使用して要約を生成
                if (messages.Count > 0)
                {
                    // メッセージをテキスト形式に変換
                    var messagesText = new StringBuilder();
                    foreach (var message in messages)
                    {
                        var timestamp = message.Ts != null 
                            ? UnixTimestampToDateTime(message.Ts).ToString("HH:mm:ss")
                            : "unknown time";
                        
                        messagesText.AppendLine($"[{timestamp}] User {message.User}: {message.Text}");
                    }
                    
                    // プロンプトを作成
                    string prompt = $"以下はSlackチャンネルの今日のメッセージです。これらのメッセージの要約を作成してください。" +
                                   $"要約は簡潔で、主要な話題やポイントを含め、Slack形式（マークダウン）で整形してください。\n\n" +
                                   $"{messagesText}";
                    
                    // 設定に基づいてAIサービスを選択
                    string aiResponse;
                    if (_useLMStudio)
                    {
                        _logger.LogInformation("Using LM Studio for summary generation");
                        aiResponse = await _lmClient.SendChatRequestAsync(prompt);
                    }
                    else
                    {
                        _logger.LogInformation("Using Azure OpenAI for summary generation");
                        aiResponse = await _aoaiClient.SendChatRequestAsync(prompt);
                    }
                    
                    // AIの応答を返す
                    if (!string.IsNullOrEmpty(aiResponse))
                    {
                        return $"*Daily Summary*\n\n{aiResponse}";
                    }
                }
                
                // AIが応答しない場合やメッセージがない場合はデフォルトの要約を作成
                int messageCount = messages.Count;
                int userCount = messages.Select(m => m.User).Distinct().Count();
                
                var summary = new StringBuilder();
                summary.AppendLine($"*Daily Summary*");
                summary.AppendLine($"Total messages today: {messageCount}");
                summary.AppendLine($"Active users: {userCount}");
                summary.AppendLine();
                
                // 例として最近のメッセージをいくつか追加
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating message summary");
                return $"*Daily Summary*\n\nError generating summary: {ex.Message}";
            }
        }

        /// <summary>
        /// UnixタイムスタンプをDateTimeに変換
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
