using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Hosting;
using Mmcc.Bot.Polychat.Jobs.Recurring.Restarts;
using Mmcc.Bot.Polychat.Services;

namespace Mmcc.Bot.Polychat.Hosting;

public class RestartNotifierBackgroundService : TimedBackgroundService<RestartNotifierBackgroundService>
{
    private readonly ILogger<RestartNotifierBackgroundService> _logger;
    private readonly IMediator _mediator;
    private readonly IPolychatService _ps;

    private static readonly TimeSpan TimeBetweenIterations = TimeSpan.FromSeconds(30);

    public RestartNotifierBackgroundService(
        ILogger<RestartNotifierBackgroundService> logger,
        IMediator mediator,
        IPolychatService ps
    ) : base(TimeBetweenIterations, logger)
    {
        _mediator = mediator;
        _ps = ps;
        _logger = logger;
    }

    protected override async Task OnExecute(CancellationToken ct)
    {
        var upcomingRestarts = await _mediator.Send(new GetUpcoming.Query(), ct);

        foreach (var (serverId, job) in upcomingRestarts)
        {
            var server = _ps.GetOnlineServerOrDefault(serverId);

            if (server is null)
            {
                _logger.LogWarning("Server {ServerId} scheduled to restart was already offline", serverId);
                return;
            }

            var timeUntilRestart = job.NextExecution!.Value - DateTime.UtcNow;

            if (timeUntilRestart >= TimeSpan.Zero)
            {
                await _mediator.Send(new Notify.Command(server, timeUntilRestart), ct);
            }
        }
    }
}