namespace Mmcc.Bot.Database.Entities
{
    /// <summary>
    /// Represents an application for the Member role.
    /// </summary>
    public class MemberApplication
    {
        /// <summary>
        /// ID of the application.
        /// </summary>
        public int MemberApplicationId { get; set; }
        
        /// <summary>
        /// ID of the guild in which the message containing the application was sent.
        /// </summary>
        public ulong GuildId { get; set; }
        
        /// <summary>
        /// ID of the channel in which the message containing the application was sent.
        /// </summary>
        public ulong ChannelId { get; set; }
        
        /// <summary>
        /// ID of the message containing the application. 
        /// </summary>
        public ulong MessageId { get; set; }
        
        /// <summary>
        /// ID of the Discord user who is the author of the message containing the application.
        /// </summary>
        public ulong AuthorDiscordId { get; set; }
        
        /// <summary>
        /// Status of the application. See <see cref="ApplicationStatus"/>.
        /// </summary>
        public ApplicationStatus AppStatus { get; set; }
        
        /// <summary>
        /// Time when the application was sent in Unix time format.
        /// </summary>
        public long AppTime { get; set; }
        
        /// <summary>
        /// Content of the message.
        /// </summary>
        public string? MessageContent { get; set; }
        
        /// <summary>
        /// URL of the image which serves as proof of fulfilling the requirements for the Member role.
        /// </summary>
        public string ImageUrl { get; set; }
        
        /// <summary>
        /// Instantiates <see cref="MemberApplication"/>
        /// </summary>
        /// <param name="guildId">ID of the guild in which the message containing the application was sent.</param>
        /// <param name="channelId">ID of the channel in which the message containing the application was sent.</param>
        /// <param name="messageId">ID of the message containing the application. </param>
        /// <param name="authorDiscordId">ID of the Discord user who is the author of the message containing the application.</param>
        /// <param name="appStatus">Status of the application. See <see cref="ApplicationStatus"/>.</param>
        /// <param name="appTime">Time when the application was sent in Unix time format.</param>
        /// <param name="imageUrl">Content of the message.</param>
        /// <param name="messageContent">URL of the image which serves as proof of fulfilling the requirements for the Member role.</param>
        public MemberApplication(
            ulong guildId,
            ulong channelId,
            ulong messageId,
            ulong authorDiscordId,
            ApplicationStatus appStatus,
            long appTime,
            string imageUrl,
            string? messageContent = null
            )
        {
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            AuthorDiscordId = authorDiscordId;
            AppStatus = appStatus;
            AppTime = appTime;
            ImageUrl = imageUrl;
            MessageContent = messageContent;
        }
    }
    
    /// <summary>
    /// Enumerates application statuses.
    /// </summary>
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}