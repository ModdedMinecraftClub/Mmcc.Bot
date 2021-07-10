using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Extensions.Database.Entities;
using Mmcc.Bot.Core.Extensions.Remora.Discord.API.Abstractions.Rest;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Infrastructure.Queries.ModerationActions;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Mmcc.Bot.Infrastructure.HostedServices
{
    /// <summary>
    /// Timed background service that deactivates moderation actions once they have expired.
    /// </summary>
    public class ModerationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<ModerationBackgroundService> _logger;
        private readonly IColourPalette _colourPalette;
        private readonly DiscordSettings _discordSettings;

        private const int TimeBetweenIterationsInMillis = 2 * 60 * 1000;

        /// <summary>
        /// Instantiates a new instance of the <see cref="ModerationBackgroundService"/> class.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="discordSettings">The Discord settings.</param>
        public ModerationBackgroundService(IServiceProvider sp, ILogger<ModerationBackgroundService> logger, IColourPalette colourPalette, DiscordSettings discordSettings)
        {
            _sp = sp;
            _logger = logger;
            _colourPalette = colourPalette;
            _discordSettings = discordSettings;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting {service}...", nameof(ModerationBackgroundService));
            _logger.LogInformation("Started {service}...", nameof(ModerationBackgroundService));
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunIterationAsync(stoppingToken);
                await Task.Delay(TimeBetweenIterationsInMillis, stoppingToken);
            }

            _logger.LogInformation("Stopped {service}...", nameof(ModerationBackgroundService));
        }

        private async Task RunIterationAsync(CancellationToken ct)
        {
            _logger.LogDebug("Running an iteration of the {service} timed background service...",
                nameof(ModerationBackgroundService));
            
            using var scope = _sp.CreateScope();
            var provider = scope.ServiceProvider;
            var guildApi = provider.GetRequiredService<IDiscordRestGuildAPI>();
            var mediator = provider.GetRequiredService<IMediator>();
            var ms = provider.GetRequiredService<IModerationService>();
            var channelApi = provider.GetRequiredService<IDiscordRestChannelAPI>();
            var getAllPendingResult = await mediator.Send(new GetActionsToDisable.Query(), ct);

            if (!getAllPendingResult.IsSuccess)
            {
                _logger.LogError(
                    "An error has occurred while running an iteration of the {service} timed background service:\n{error}",
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
                        "An error has occurred while running an iteration of the {service} timed background service:\n{error}",
                        nameof(ModerationBackgroundService),
                        getLogsChannel.Error
                    );
                    break;
                }

                var deactivateResult = await ms.Deactivate(ma, getLogsChannel.Entity.ID);

                if (!deactivateResult.IsSuccess)
                {
                    _logger.LogError(
                        "An error has occurred while running an iteration of the {service} timed background service:\n{error}",
                        nameof(ModerationBackgroundService),
                        deactivateResult.Error
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
                        "Successfully deactivated expired moderation action with ID: {id} but failed to send a notification to the logs channel." +
                        "It may be because the bot doesn't have permissions in that channel or has since been removed from the guild. This warning can in most cases be ignored." +
                        "The error was:\n{error}",
                        ma.ModerationActionId,
                        sendNotificationResult.Error
                    );
                    break;
                }

                _logger.LogInformation(
                    "Successfully deactivated expired moderation action with ID: {id}.", ma.ModerationActionId);
            }

            _logger.LogDebug("Completed an iteration of the {service} timed background service.",
                nameof(ModerationBackgroundService));
        }
    }
}