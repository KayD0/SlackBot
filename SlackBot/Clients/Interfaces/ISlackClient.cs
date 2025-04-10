using SlackBot.ResponseModel.SlackApi;

namespace SlackBot.Clients.Interfaces
{
    /// <summary>
    /// Interface for interacting with the Slack API
    /// </summary>
    public interface ISlackClient
    {
        /// <summary>
        /// Retrieves all channels that the bot is a member of
        /// </summary>
        /// <returns>List of channels</returns>
        Task<List<Channel>> GetBotChannels();

        /// <summary>
        /// Retrieves chat history for a specific channel on a specific date
        /// </summary>
        /// <param name="channelId">The ID of the channel</param>
        /// <param name="date">The date to retrieve history for</param>
        /// <returns>List of messages</returns>
        Task<List<Message>> GetChannelHistory(string channelId, DateTime date);

        /// <summary>
        /// Retrieves a list of all users in the Slack workspace
        /// </summary>
        /// <returns>List of Slack users</returns>
        Task<List<SlackUser>> GetUserList();

        /// <summary>
        /// Sends a message to a specified channel
        /// </summary>
        /// <param name="channelId">The ID of the channel to send the message to</param>
        /// <param name="message">The message text to send</param>
        /// <returns>True if the message was sent successfully, false otherwise</returns>
        Task<bool> SendMessageToChannel(string channelId, string message);
    }
}
