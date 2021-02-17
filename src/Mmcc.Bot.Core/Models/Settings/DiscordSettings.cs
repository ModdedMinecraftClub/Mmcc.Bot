﻿namespace Mmcc.Bot.Core.Models.Settings
{
    /// <summary>
    /// Discord API settings.
    /// </summary>
    public class DiscordSettings
    {
        /// <summary>
        /// Prefix used for commands.
        /// </summary>
        public char Prefix { get; set; }
        
        /// <summary>
        /// Discord API token.
        /// </summary>
        public string Token { get; set; } = null!;
        
        /// <summary>
        /// Channel names settings.
        /// </summary>
        public ChannelNamesSettings ChannelNames { get; set; } = null!;
        
        /// <summary>
        /// Role names settings.
        /// </summary>
        public RoleNamesSettings RoleNames { get; set; } = null!;
    }
}