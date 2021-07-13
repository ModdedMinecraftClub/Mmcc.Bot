namespace Mmcc.Bot.Common.Models.Settings
{
    /// <summary>
    /// Channel names settings.
    /// </summary>
    public class ChannelNamesSettings
    {
        /// <summary>
        /// Name of the channel where members post applications.
        /// </summary>
        public string MemberApps { get; set; } = null!;
        
        /// <summary>
        /// Name of the channel to which the bot will send moderation action logs.
        /// </summary>
        public string ModerationLogs { get; set; } = null!;

        /// <summary>
        /// Name of the channel where to which the bot will send general logs.
        /// </summary>
        public string LogsSpam { get; set; } = null!;
    }
}