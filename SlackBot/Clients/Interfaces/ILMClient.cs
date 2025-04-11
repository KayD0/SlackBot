using System.Threading.Tasks;

namespace SlackBot.Clients.Interfaces
{
    /// <summary>
    /// LM Studioと対話するためのインターフェース
    /// </summary>
    public interface ILMClient
    {
        /// <summary>
        /// LM Studioにチャットリクエストを送信します
        /// </summary>
        /// <param name="prompt">ユーザーのメッセージ/プロンプト</param>
        /// <returns>AI応答テキスト</returns>
        Task<string> SendChatRequestAsync(string prompt);
    }
}
