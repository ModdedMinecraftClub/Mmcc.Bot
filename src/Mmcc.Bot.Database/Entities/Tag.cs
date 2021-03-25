namespace Mmcc.Bot.Database.Entities
{
    /// <summary>
    /// Represents a tag.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// ID of the guild.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// Name of the tag.
        /// </summary>
        public string TagName { get; set; } = null!;
        
        /// <summary>
        /// Description of the tag.
        ///
        /// Set to <code>null</code> if no description.
        /// </summary>
        public string? TagDescription { get; set; }

        /// <summary>
        /// Content of the tag.
        /// </summary>
        ///
        /// <remarks>Content meaning what the bot sends in the message once the tag is invoked.</remarks>
        public string Content { get; set; } = null!;
        
        /// <summary>
        /// When the tag was created.
        /// </summary>
        public long CreatedAt { get; set; }
        
        /// <summary>
        /// When the tag was last modified.
        ///
        /// Set to <code>null</code> if the tag has not been modified since its creation.
        /// </summary>
        public long? LastModifiedAt { get; set; }
        
        /// <summary>
        /// Discord ID of the user that created the tag.
        /// </summary>
        public ulong CreatedByDiscordId { get; set; }
        
        /// <summary>
        /// Discord ID of the user that has last modified the tag.
        ///
        /// Set to <code>null</code> if the tag has not been modified since its creation.
        /// </summary>
        public ulong? LastModifiedByDiscordId { get; set; }

        /// <summary>
        /// Instantiates a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="guildId">ID of the guild.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="content">Content of the tag.</param>
        /// <param name="createdAt">When the tag was created.</param>
        /// <param name="createdByDiscordId">Discord ID of the user that created the tag.</param>
        /// <param name="tagDescription">Description of the tag. Set to <code>null</code> if no description.</param>
        /// <param name="lastModifiedAt">When the tag was last modified. Set to <code>null</code> if the tag has not been modified since its creation.</param>
        /// <param name="lastModifiedByDiscordId">Discord ID of the user that has last modified the tag. Set to <code>null</code> if the tag has not been modified since its creation.</param>
        public Tag(
            ulong guildId,
            string tagName,
            string content,
            long createdAt,
            ulong createdByDiscordId,
            string? tagDescription = null,
            long? lastModifiedAt = null,
            ulong? lastModifiedByDiscordId = null
        )
        {
            GuildId = guildId;
            TagName = tagName;
            Content = content;
            CreatedAt = createdAt;
            CreatedByDiscordId = createdByDiscordId;
            TagDescription = tagDescription;
            LastModifiedAt = lastModifiedAt;
            LastModifiedByDiscordId = lastModifiedByDiscordId;
        }
    }
}