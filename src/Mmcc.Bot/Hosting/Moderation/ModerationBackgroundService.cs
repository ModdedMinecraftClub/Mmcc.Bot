using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Commands.Moderation.Bans;
using Mmcc.Bot.Common.Extensions.Database.Entities;
using Mmcc.Bot.Common.Extensions.Remora.Discord.API.Abstractions.Rest;
using Mmcc.Bot.Common.Hosting;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Results;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Hosting.Moderation;

/// <summary>
/// Timed background service that deactivates moderation actions once they have expired.
/// </summary>
public class ModerationBackgroundService : TimedBackgroundService<ModerationBackgroundService>
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ModerationBackgroundService> _logger;
    private readonly IColourPalette _colourPalette;
    private readonly DiscordSettings _discordSettings;

    private const int TimeBetweenIterationsInMillis = 2 * 60 * 1000;
    
    public ModerationBackgroundService(
        IServiceProvider sp,
        ILogger<ModerationBackgroundService> logger,
        IColourPalette colourPalette,
        DiscordSettings discordSettings
    ) : base(TimeBetweenIterationsInMillis, logger)
    {
        _sp = sp;
        _logger = logger;
        _colourPalette = colourPalette;
        _discordSettings = discordSettings;
    }

    protected override async Task OnExecute(CancellationToken ct)
    {
        _logger.LogDebug("Running an iteration of the {Service} timed background service...",
            nameof(ModerationBackgroundService));
            
        using var scope = _sp.CreateScope();
        var provider = scope.ServiceProvider;
        var guildApi = provider.GetRequiredService<IDiscordRestGuildAPI>();
        var mediator = provider.GetRequiredService<IMediator>();
        var channelApi = provider.GetRequiredService<IDiscordRestChannelAPI>();
        var getAllPendingResult = await mediator.Send(new GetExpiredActions.Query(), ct);

        if (!getAllPendingResult.IsSuccess)
        {
            _logger.LogError(
                "An error has occurred while running an iteration of the {Service} timed background service:\n{Error}",
                nameof(ModerationBackgroundService),
                getAllPendingResult.Error
            );
            return;
        }

        var actionsToDeactivate = getAllPendingResult.Entity;

        foreach (var ma in actionsToDeactivate)
        {
            var getLogsChannel = await guildApi.FindGuildChannelByName(new Snowflake(ma.GuildId),
                _discordSettings.ChannelNames.ModerationLogs);
            if (!getLogsChannel.IsSuccess)
            {
                _logger.LogError(
                    "An error has occurred while running an iteration of the {Service} timed background service:\n{Error}",
                    nameof(ModerationBackgroundService),
                    getLogsChannel.Error
                );
                break;
            }
                
            Result<ModerationAction> unbanResult = ma.ModerationActionType switch
            {
                ModerationActionType.Ban => await mediator.Send(new Unban.Command
                    { ModerationAction = ma, ChannelId = getLogsChannel.Entity.ID }, ct),

                _ => Result<ModerationAction>.FromError(new UnsupportedFeatureError("Unsupported moderation type."))
            };
            if (!unbanResult.IsSuccess)
            {
                _logger.LogError(
                    "An error has occurred while running an iteration of the {Service} timed background service:\n{Error}",
                    nameof(ModerationBackgroundService),
                    unbanResult.Error
                );
                break;
            }

            var typeString = ma.ModerationActionType.ToStringWithEmoji();
            var userSb = new StringBuilder();

            if (ma.UserDiscordId is not null)
            {
                userSb.AppendLine($"Discord user: <@{ma.UserDiscordId}>");
            }

            if (ma.UserIgn is not null)
            {
                userSb.AppendLine($"IGN: `{ma.UserIgn}`");
            }

            var notificationEmbed = new Embed
            {
                Title = $"Moderation action with ID: {ma.ModerationActionId} has expired.",
                Description = "Moderation action has expired and has therefore been deactivated.",
                Colour = _colourPalette.Green,
                Fields = new List<EmbedField>
                {
                    new("Action type", typeString, false),
                    new("User info", userSb.ToString(), false)
                },
                Timestamp = DateTimeOffset.UtcNow
            };
            var sendNotificationResult = await channelApi.CreateMessageAsync(getLogsChannel.Entity.ID,
                embeds: new[] { notificationEmbed }, ct: ct);
            if (!sendNotificationResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Successfully deactivated expired moderation action with ID: {Id} but failed to send a notification to the logs channel." +
                    "It may be because the bot doesn't have permissions in that channel or has since been removed from the guild. This warning can in most cases be ignored." +
                    "The error was:\n{Error}",
                    ma.ModerationActionId,
                    sendNotificationResult.Error
                );
                break;
            }

            _logger.LogInformation(
                "Successfully deactivated expired moderation action with ID: {Id}", ma.ModerationActionId);
        }

        _logger.LogDebug("Completed an iteration of the {Service} timed background service",
            nameof(ModerationBackgroundService));
    }
}