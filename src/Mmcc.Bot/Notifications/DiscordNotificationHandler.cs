using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common;
using Mmcc.Bot.Common.Extensions.Remora.Discord.API.Abstractions.Rest;
using Mmcc.Bot.Common.Models;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Common.UI.Embeds;
using Remora.Discord.API.Abstractions.Rest;

namespace Mmcc.Bot.Notifications;

[ExcludeFromMediatrAssemblyScan]
public class DiscordNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : IMmccNotification, IDiscordNotifiable
{
    private readonly ILogger<DiscordNotificationHandler<TNotification>> _logger;
    private readonly DiscordSettings _discordSettings;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IDiscordRestGuildAPI _guildApi;

    public DiscordNotificationHandler(
        ILogger<DiscordNotificationHandler<TNotification>> logger,
        DiscordSettings discordSettings,
        IDiscordRestChannelAPI channelApi,
        IDiscordRestGuildAPI guildApi
    )
    {
        _logger = logger;
        _discordSettings = discordSettings;
        _channelApi = channelApi;
        _guildApi = guildApi;
    }

    public async Task Handle(TNotification notification, CancellationToken cancellationToken)
    {
        // TODO: cache this;
        var logsChannelName = _discordSettings.ChannelNames.ModerationLogs;
        var getLogsChannel = await _guildApi.FindGuildChannelByName(notification.TargetGuildId, logsChannelName);
        if (!getLogsChannel.IsSuccess)
        {
            _logger.LogError(
                "An error has occurred while running an iteration of the {Service} timed background service:\n{Error}",
                nameof(DiscordNotificationHandler<TNotification>),
                getLogsChannel.Error
            );
        }

        var notificationEmbed = new NotificationEmbed(notification);
        var sendNotificationResult = await _channelApi.CreateMessageAsync(getLogsChannel.Entity.ID,
            embeds: new[] { notificationEmbed }, ct: cancellationToken);
        if (!sendNotificationResult.IsSuccess)
        {
            _logger.LogWarning(
                "Successfully deactivated expired moderation action but failed to send a notification to the logs channel {LogsChannelName} in guild {GuildId}." +
                "It may be because the bot doesn't have permissions in that channel or has since been removed from the guild. This warning can in most cases be ignored." +
                "The error was:\n{Error}",
                logsChannelName,
                notification.TargetGuildId,
                sendNotificationResult.Error
            );
        }
    }
}