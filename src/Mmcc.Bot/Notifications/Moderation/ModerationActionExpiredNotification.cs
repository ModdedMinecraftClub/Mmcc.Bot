using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Extensions.Database.Entities;
using Mmcc.Bot.Common.Models;
using Mmcc.Bot.Database.Entities;

namespace Mmcc.Bot.Notifications.Moderation;

public record ModerationActionExpiredNotification : Notification
{
    public ModerationActionExpiredNotification(ModerationAction ma) : base(
        $"Moderation action with ID: {ma.ModerationActionId} has expired.",
        "Moderation action has expired and has therefore been deactivated.",
        DateTimeOffset.UtcNow,
        new List<KeyValuePair<string, string>>
        {
            new("Action type", ma.ModerationActionType.ToStringWithEmoji()),
            new("User info", ma.GetUserDataDisplayString())
        }
    )
    {
    }
}

public class DiscordNotificationHandler : INotificationHandler<ModerationActionExpiredNotification>
{
    public async Task Handle(ModerationActionExpiredNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("a");
    }
}