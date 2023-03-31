using System;
using System.Collections.Generic;
using Mmcc.Bot.Common.Extensions.Database.Entities;
using Mmcc.Bot.Common.Models;
using Mmcc.Bot.Database.Entities;
using Remora.Rest.Core;

namespace Mmcc.Bot.Notifications.Moderation;

public record ModerationActionExpiredNotification(
    string Title,
    string? Description,
    DateTimeOffset? Timestamp,
    IReadOnlyList<KeyValuePair<string, string>>? CustomProperties,
    Snowflake TargetGuildId
) : IMmccNotification, IDiscordNotifiable
{
    public ModerationActionExpiredNotification(ModerationAction ma)
        : this(
            Title: $"Moderation action with ID: {ma.ModerationActionId} has expired.",
            Description: "Moderation action has expired and has therefore been deactivated.",
            Timestamp: DateTimeOffset.UtcNow,
            TargetGuildId: new(ma.GuildId),
            CustomProperties: new List<KeyValuePair<string, string>>
            {
                new("Action type", ma.ModerationActionType.ToStringWithEmoji()),
                new("User info", ma.GetUserDataDisplayString())
            }
        )
    {
    }
}