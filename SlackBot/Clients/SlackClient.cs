using SlackBot.Clients.Interfaces;
using SlackBot.ResponseModel.SlackApi;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SlackBot.Clients
{
    /// <summary>
    /// Slack APIと対話するためのサービス
    /// </summary>
    public class SlackClient : ISlackClient
    {
        private readonly HttpClient _slackApiClient;
        private const string HttpClientName = "SlackApiClient";

        /// <summary>
        /// SlackServiceクラスの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="httpClientFactory">名前付きクライアントを作成するためのHttpClientFactory</param>
        public SlackClient(IHttpClientFactory httpClientFactory)
        {
            _slackApiClient = httpClientFactory.CreateClient(HttpClientName);
        }

        /// <summary>
        /// ボットがメンバーになっているすべてのチャンネルを取得
        /// </summary>
        /// <returns>チャンネルのリスト</returns>
        public async Task<List<Channel>> GetBotChannels()
        {
            // Slack APIを呼び出して会話リストを取得
            var response = await _slackApiClient.GetAsync("https://slack.com/api/conversations.list?types=public_channel,private_channel&limit=1000");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var conversationsResponse = JsonSerializer.Deserialize<ConversationsListResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (conversationsResponse == null)
            {
                throw new Exception("Slack APIレスポンスのデシリアライズに失敗しました");
            }

            if (!conversationsResponse.Ok)
            {
                throw new Exception($"Slack APIエラー: {conversationsResponse.Error}");
            }

            // ボットがメンバーになっているチャンネルのみをフィルタリング
            var botChannels = new List<Channel>();

            if (conversationsResponse.Channels != null)
            {
                foreach (var channel in conversationsResponse.Channels)
                {
                    // ボットがこのチャンネルのメンバーかどうかを確認
                    if (channel.IsMember)
                    {
                        botChannels.Add(channel);
                    }
                }
            }

            return botChannels;
        }

        /// <summary>
        /// 特定のチャンネルの特定の日付のチャット履歴を取得
        /// </summary>
        /// <param name="channelId">チャンネルのID</param>
        /// <param name="date">履歴を取得する日付</param>
        /// <returns>メッセージのリスト</returns>
        public async Task<List<Message>> GetChannelHistory(string channelId, DateTime date)
        {
            // 日の始まりと終わりのUnixタイムスタンプを計算
            DateTime startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local);
            DateTime endOfDay = startOfDay.AddDays(1).AddSeconds(-1);

            double oldest = DateTimeToUnixTimestamp(startOfDay);
            double latest = DateTimeToUnixTimestamp(endOfDay);

            // Slack APIを呼び出して会話履歴を取得
            string url = $"https://slack.com/api/conversations.history?channel={channelId}&oldest={oldest}&latest={latest}&limit=1000";
            var response = await _slackApiClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var historyResponse = JsonSerializer.Deserialize<ConversationHistoryResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (historyResponse == null)
            {
                throw new Exception("Slack APIレスポンスのデシリアライズに失敗しました");
            }

            if (!historyResponse.Ok)
            {
                throw new Exception($"Slack APIエラー: {historyResponse.Error}");
            }

            return historyResponse.Messages ?? new List<Message>();
        }

        /// <summary>
        /// Slackワークスペース内のすべてのユーザーのリストを取得
        /// </summary>
        /// <returns>Slackユーザーのリスト</returns>
        public async Task<List<SlackUser>> GetUserList()
        {
            // Slack APIを呼び出してユーザーリストを取得
            var response = await _slackApiClient.GetAsync("https://slack.com/api/users.list");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var usersResponse = JsonSerializer.Deserialize<UsersListResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (usersResponse == null)
            {
                throw new Exception("Slack APIレスポンスのデシリアライズに失敗しました");
            }

            if (!usersResponse.Ok)
            {
                throw new Exception($"Slack APIエラー: {usersResponse.Error}");
            }

            return usersResponse.Members ?? new List<SlackUser>();
        }

        /// <summary>
        /// 指定されたチャンネルにメッセージを送信
        /// </summary>
        /// <param name="channelId">メッセージを送信するチャンネルのID</param>
        /// <param name="message">送信するメッセージテキスト</param>
        /// <returns>メッセージが正常に送信された場合はtrue、それ以外の場合はfalse</returns>
        public async Task<bool> SendMessageToChannel(string channelId, string message)
        {
            try
            {
                // リクエストコンテンツを作成
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("channel", channelId),
                    new KeyValuePair<string, string>("text", message)
                });

                // Slack APIを呼び出してメッセージを送信
                var response = await _slackApiClient.PostAsync("https://slack.com/api/chat.postMessage", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var postMessageResponse = JsonSerializer.Deserialize<SlackApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (postMessageResponse == null)
                {
                    Console.WriteLine("Slack APIレスポンスのデシリアライズに失敗しました");
                    return false;
                }

                if (!postMessageResponse.Ok)
                {
                    Console.WriteLine($"Slack APIエラー: {postMessageResponse.Error}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"メッセージ送信エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// DateTimeをUnixタイムスタンプ（エポックからの秒数）に変換
        /// </summary>
        private static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (dateTime.ToUniversalTime() - epoch).TotalSeconds;
        }

        /// <summary>
        /// UnixタイムスタンプをDateTimeに変換
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
