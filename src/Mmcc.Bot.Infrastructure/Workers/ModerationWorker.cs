using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Infrastructure.Queries.Discord;
using Mmcc.Bot.Infrastructure.Queries.ModerationActions;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Mmcc.Bot.Infrastructure.Workers
{
    /// <summary>
    /// Timed background service that deactivates moderation actions once they have expired.
    /// </summary>
    ///
    /// <inheritdoc cref="BackgroundService"/>
    public class ModerationWorker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<ModerationWorker> _logger;
        private readonly ColourPalette _colourPalette;

        private const int TimeBetweenIterationsInMillis = 10 * 1000;

        /// <summary>
        /// Instantiates a new instance of the <see cref="ModerationWorker"/> class.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public ModerationWorker(IServiceProvider sp, ILogger<ModerationWorker> logger, ColourPalette colourPalette)
        {
            _sp = sp;
            _logger = logger;
            _colourPalette = colourPalette;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Started {nameof(ModerationWorker)}...");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                stoppingToken.ThrowIfCancellationRequested();
                await RunIterationAsync(stoppingToken);
                await Task.Delay(TimeBetweenIterationsInMillis, stoppingToken);
            }

            _logger.LogInformation($"Stopped {nameof(ModerationWorker)}");
        }

        private async Task RunIterationAsync(CancellationToken ct)
        {
            _logger.LogInformation($"{nameof(ModerationWorker)} is running an iteration...");
            
            using var scope = _sp.CreateScope();
            var provider = scope.ServiceProvider;
            var mediator = provider.GetRequiredService<IMediator>();
            var ms = provider.GetRequiredService<IModerationService>();
            var channelApi = provider.GetRequiredService<IDiscordRestChannelAPI>();
            var getAllPendingResult = await mediator.Send(new GetAllActive.Query(), ct);

            if (!getAllPendingResult.IsSuccess)
            {
                _logger.LogError($"Error in the moderation worker.\n{getAllPendingResult.Error}");
                return;
            }

            var actionsToDeactivate = getAllPendingResult.Entity
                .Where(ma =>
                    ma.ModerationActionType != ModerationActionType.Mute
                    && ma.ExpiryDate is not null
                    && DateTimeOffset.FromUnixTimeMilliseconds(ma.ExpiryDate.Value) < DateTimeOffset.Now);

            foreach (var ma in actionsToDeactivate)
            {
                var getLogsChannel = await mediator.Send(new GetChannelByName.Query
                {
                    ChannelName = "logs",
                    GuildId = new Snowflake(ma.GuildId)
                }, ct);
                if (!getLogsChannel.IsSuccess)
                {
                    _logger.LogError($"Error in {nameof(ModerationWorker)}" + "\n" + getLogsChannel.Error);
                    break;
                }

                var deactivateResult = await ms.Deactivate(ma, getLogsChannel.Entity.ID);

                if (!deactivateResult.IsSuccess)
                {
                    _logger.LogError($"Error in the moderation worker.\n{deactivateResult.Error}");
                    break;
                }

                var warningMsg =
                    $"Successfully deactivated expired moderation action with ID: {ma.ModerationActionId} but failed to send a notification to the logs channel. It may be because the bot doesn't have permissions in that channel or has since been removed from the guild. This warning can in most cases be ignored.";

                var typeString = ma.ModerationActionType switch
                {
                    ModerationActionType.Mute => $":no_mouth: {ma.ModerationActionType.ToString()}",
                    ModerationActionType.Ban => $":no_pedestrians: {ma.ModerationActionType.ToString()}",
                    _ => "`Unsupported`"
                };
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
                    embed: notificationEmbed, ct: ct);
                if (!sendNotificationResult.IsSuccess)
                {
                    _logger.LogWarning(warningMsg + "\n" + sendNotificationResult.Error);
                    break;
                }

                _logger.LogInformation(
                    $"Successfully deactivated expired moderation action with ID: {ma.ModerationActionId}");
            }

            _logger.LogInformation($"{nameof(ModerationWorker)} has completed an iteration.");
        }
    }
}