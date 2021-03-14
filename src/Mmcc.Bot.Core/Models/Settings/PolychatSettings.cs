namespace Mmcc.Bot.Core.Models.Settings
{
    /// <summary>
    /// Settings for communication with Polychat2.
    /// </summary>
    public class PolychatSettings
    {
        /// <summary>
        /// Port on which we will listen to incoming Polychat2 messages.
        /// </summary>
        public int Port { get; set; }
        
        /// <summary>
        /// The limit of the message queue.
        /// </summary>
        public int MessageQueueLimit { get; set; }
        
        /// <summary>
        /// ID of the channel where MC chat will be forwarded to.
        /// </summary>
        public ulong ChatChannelId { get; set; }

        /// <summary>
        /// Settings for broadcasts.
        /// </summary>
        public BroadcastsSettings? BroadcastsSettings { get; set; }
    }
}