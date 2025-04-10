using SlackBot.ResponseModel.SlackApi;

namespace SlackBot.Clients.Interfaces
{
    /// <summary>
    /// Slack APIと対話するためのインターフェース
    /// </summary>
    public interface ISlackClient
    {
        /// <summary>
        /// ボットがメンバーになっているすべてのチャンネルを取得
        /// </summary>
        /// <returns>チャンネルのリスト</returns>
        Task<List<Channel>> GetBotChannels();

        /// <summary>
        /// 特定のチャンネルの特定の日付のチャット履歴を取得
        /// </summary>
        /// <param name="channelId">チャンネルのID</param>
        /// <param name="date">履歴を取得する日付</param>
        /// <returns>メッセージのリスト</returns>
        Task<List<Message>> GetChannelHistory(string channelId, DateTime date);

        /// <summary>
        /// Slackワークスペース内のすべてのユーザーのリストを取得
        /// </summary>
        /// <returns>Slackユーザーのリスト</returns>
        Task<List<SlackUser>> GetUserList();

        /// <summary>
        /// 指定されたチャンネルにメッセージを送信
        /// </summary>
        /// <param name="channelId">メッセージを送信するチャンネルのID</param>
        /// <param name="message">送信するメッセージテキスト</param>
        /// <returns>メッセージが正常に送信された場合はtrue、それ以外の場合はfalse</returns>
        Task<bool> SendMessageToChannel(string channelId, string message);
    }
}
