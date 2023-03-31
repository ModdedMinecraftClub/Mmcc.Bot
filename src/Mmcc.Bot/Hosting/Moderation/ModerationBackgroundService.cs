using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Commands.Moderation.Bans;
using Mmcc.Bot.Common.Hosting;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.Commands.Results;
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
    
    private static readonly TimeSpan TimeBetweenIterations = TimeSpan.FromMinutes(2);
    
    public ModerationBackgroundService(
        IServiceProvider sp,
        ILogger<ModerationBackgroundService> logger,
        IColourPalette colourPalette,
        DiscordSettings discordSettings
    ) : base(TimeBetweenIterations, logger)
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
        var mediator = provider.GetRequiredService<IMediator>();

        var getAllPendingResult = await mediator.Send(new GetExpiredActions.Query(), ct);
        if (!getAllPendingResult.IsSuccess)
        {
            _logger.LogError(
                "An error has occurred while getting expired modification actions as part of hosted service: {HostedServiceName}:\n{Error}",
                nameof(ModerationBackgroundService),
                getAllPendingResult.Error
            );
            return;
        }

        var actionsToDeactivate = getAllPendingResult.Entity;

        foreach (var ma in actionsToDeactivate)
        {
            var unbanResult = ma.ModerationActionType switch
            {
                ModerationActionType.Ban => await mediator.Send(new Unban.Command { ModerationAction = ma }, ct),

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

            _logger.LogInformation(
                "Successfully deactivated expired moderation action with ID: {Id}", ma.ModerationActionId);
        }

        _logger.LogDebug("Completed an iteration of the {Service} timed background service",
            nameof(ModerationBackgroundService));
    }
}