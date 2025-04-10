namespace SlackBot.Clients.Interfaces
{
    /// <summary>
    /// Azure OpenAI APIと対話するためのインターフェース
    /// </summary>
    public interface IAOAIClient
    {
        /// <summary>
        /// Azure OpenAIサービスにチャットリクエストを送信します
        /// </summary>
        /// <param name="prompt">ユーザーのメッセージ/プロンプト</param>
        /// <returns>AI応答テキスト</returns>
        Task<string> SendChatRequestAsync(string prompt);
    }
}
