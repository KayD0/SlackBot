namespace SlackBot.Clients.Interfaces
{
    /// <summary>
    /// Interface for interacting with the Azure OpenAI API
    /// </summary>
    public interface IAOAIClient
    {
        /// <summary>
        /// Sends a chat request to Azure OpenAI service
        /// </summary>
        /// <param name="prompt">The user's message/prompt</param>
        /// <returns>The AI response text</returns>
        Task<string> SendChatRequestAsync(string prompt);
    }
}
