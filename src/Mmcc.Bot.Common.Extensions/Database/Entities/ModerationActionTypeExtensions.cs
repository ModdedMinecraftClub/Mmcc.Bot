using Mmcc.Bot.Database.Entities;

namespace Mmcc.Bot.Common.Extensions.Database.Entities
{
    /// <summary>
    /// Extensions for <see cref="ModerationAction"/>.
    /// </summary>
    public static class ModerationActionTypeExtensions
    {
        /// <summary>
        /// Converts to a <see cref="string"/> with the corresponding Discord emoji.
        /// </summary>
        /// <param name="moderationActionType">Moderation action type.</param>
        /// <returns>A <see cref="string"/> with the corresponding Discord emoji.</returns>
        public static string ToStringWithEmoji(this ModerationActionType moderationActionType) =>
            moderationActionType switch
            {
                ModerationActionType.Warn => $":warning: {moderationActionType.ToString()}",
                ModerationActionType.Mute => $":no_mouth: {moderationActionType.ToString()}",
                ModerationActionType.Ban => $":no_pedestrians: {moderationActionType.ToString()}",
                _ => "`Unsupported`"
            };
    }
}