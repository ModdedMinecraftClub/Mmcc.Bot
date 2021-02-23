namespace Mmcc.Bot.Database.Entities
{
    /// <summary>
    /// Represents a moderation action.
    /// </summary>
    public class ModerationAction
    {
        /// <summary>
        /// ID of the moderation action.
        /// </summary>
        public int ModerationActionId { get; set; }
        
        /// <summary>
        /// Type of the moderation action. See <see cref="ModerationActionType"/>.
        /// </summary>
        public ModerationActionType ModerationActionType { get; set; }
        
        /// <summary>
        /// Whether the moderation action is active.
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Date when the moderation action was taken in Unix time format.
        /// </summary>
        public long Date { get; set; }
        
        /// <summary>
        /// Expiry date of the moderation action in Unix time format. Set to <code>null</code> if permanent.
        /// </summary>
        public long? ExpiryDate { get; set; }
        
        /// <summary>
        /// The user's Discord ID. Set to <code>null</code> if not associated with a Discord user.
        /// </summary>
        public ulong? UserDiscordId { get; set; }
        
        /// <summary>
        /// The user's IGN. Set to <code>null</code> if not associated with an in-game user.
        /// </summary>
        public string? UserIgn { get; set; }
        
        /// <summary>
        /// Reason for the moderation action.
        /// </summary>
        public string Reason { get; set; }
        
        /// <summary>
        /// Instantiates <see cref="ModerationAction"/>.
        /// </summary>
        /// <param name="moderationActionType">Moderation action type.</param>
        /// <param name="isActive">Whether the moderation action is active.</param>
        /// <param name="date">Date when the moderation action was taken in Unix time format.</param>
        /// <param name="reason">Reason for the moderation action.</param>
        /// <param name="userDiscordId">The user's Discord ID. Set to <code>null</code> if not associated with a Discord user.</param>
        /// <param name="userIgn">The user's IGN. Set to <code>null</code> if not associated with an in-game user.</param>
        /// <param name="expiryDate">Expiry date of the moderation action in Unix time format. Set to <code>null</code> if permanent.</param>
        public ModerationAction(
            ModerationActionType moderationActionType,
            bool isActive,
            string reason,
            long date,
            ulong? userDiscordId = null,
            string? userIgn = null,
            long? expiryDate = null 
        )
        {
            ModerationActionType = moderationActionType;
            IsActive = isActive;
            Reason = reason;
            Date = date;
            UserDiscordId = userDiscordId;
            UserIgn = userIgn;
            ExpiryDate = expiryDate;
        }
    }
    
    /// <summary>
    /// Enumerates moderation types.
    /// </summary>
    public enum ModerationActionType
    {
        Warn,
        Mute,
        Ban
    }
}