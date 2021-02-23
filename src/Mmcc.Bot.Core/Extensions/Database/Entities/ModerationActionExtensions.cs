using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mmcc.Bot.Core.Extensions.System;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Core.Extensions.Database.Entities
{
    public static class ModerationActionExtensions
    {
        /// <summary>
        /// Converts an enumerable of moderation actions into an enumerable of formatted embed fields that represent the moderation actions.
        /// </summary>
        /// <param name="moderationActions">Moderation actions</param>
        /// <param name="showAssociatedDiscord">Whether to include associated Discord user in each moderation action field value.</param>
        /// <param name="showAssociatedIgn">Whether to include associated IGN in each moderation action field value.</param>
        /// <returns>Enumerable of formatted embed fields that represent the moderation actions.</returns>
        public static IEnumerable<EmbedField> ToEmbedFields(this IEnumerable<ModerationAction> moderationActions, bool showAssociatedDiscord, bool showAssociatedIgn)
        {
            var moderationActionsList = moderationActions.ToList();
            var warningsField = moderationActionsList.GetEmbedFieldForActionsOfType(ModerationActionType.Warn,
                showAssociatedDiscord, showAssociatedIgn);
            var mutesField = moderationActionsList.GetEmbedFieldForActionsOfType(ModerationActionType.Mute,
                showAssociatedDiscord, showAssociatedIgn);
            var bansField = moderationActionsList.GetEmbedFieldForActionsOfType(ModerationActionType.Ban,
                showAssociatedDiscord, showAssociatedIgn);
            return new[] {warningsField, mutesField, bansField};
        }

        private static EmbedField GetEmbedFieldForActionsOfType(this IEnumerable<ModerationAction> moderationActions, ModerationActionType type, bool showAssociatedDiscord, bool showAssociatedIgn)
        {
            var list = moderationActions.Where(ma => ma.ModerationActionType == type).ToList();
            var fieldValue = new StringBuilder();

            if (!list.Any())
            {
                fieldValue.AppendLine($"This user has no {type.ToString().ToLower()}s");
            }
            else
            {
                foreach (var moderationAction in list)
                {
                    fieldValue.AppendLine($"**▸ #{moderationAction.ModerationActionId}**");
                    
                    if (showAssociatedDiscord)
                    {
                        fieldValue.AppendLine(moderationAction.UserDiscordId is null
                            ? "Associated Discord user: `None`"
                            : $"Associated Discord user: <@{moderationAction.UserDiscordId}>");
                    }
                    
                    if (showAssociatedIgn)
                    {
                        fieldValue.AppendLine(moderationAction.UserDiscordId is null
                            ? "Associated IGN user: `None`"
                            : $"Associated Discord user: <@{moderationAction.UserIgn}>");
                    }
                    
                    fieldValue.AppendLine($"Active: {moderationAction.IsActive}");
                    fieldValue.AppendLine($"Issued at: {DateTimeOffset.FromUnixTimeMilliseconds(moderationAction.Date).UtcDateTime} UTC");
                    
                    if (moderationAction.ExpiryDate is not null)
                    {
                        fieldValue.AppendLine(moderationAction.IsActive
                            ? $"Expires at: {DateTimeOffset.FromUnixTimeMilliseconds(moderationAction.ExpiryDate.Value).UtcDateTime} UTC"
                            : $"Expired at: {DateTimeOffset.FromUnixTimeMilliseconds(moderationAction.ExpiryDate.Value).UtcDateTime} UTC");
                    }
                    else
                    {
                        fieldValue.AppendLine("Expires at: `Permanent`");
                    }

                    var splitReason = moderationAction.Reason.SplitByNewLine();
                    var reason = new StringBuilder();
                    
                    foreach (var reasonLine in splitReason)
                    {
                        reason.AppendLine("> " + reasonLine);
                    }
                    
                    fieldValue.AppendLine($"Reason:\n {reason}");
                    fieldValue.AppendLine();
                }
            }

            var title = type switch
            {
                ModerationActionType.Warn => ":warning: Warnings",
                ModerationActionType.Mute => ":no_mouth: Mutes",
                ModerationActionType.Ban => ":no_pedestrians: Bans",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            return new EmbedField(title, fieldValue.ToString(), false);
        }
    }
}