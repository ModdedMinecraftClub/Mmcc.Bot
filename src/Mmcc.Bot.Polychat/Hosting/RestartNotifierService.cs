using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Hosting;
using Mmcc.Bot.Polychat.Notifications;
using Mmcc.Bot.Polychat.Services;

namespace Mmcc.Bot.Polychat.Hosting;

public class RestartNotifierService : TimedBackgroundService<RestartNotifierService>
{
    private const int TimeBetweenIterationsInMillis = 30 * 1000;

    private readonly ILogger<RestartNotifierService> _logger;
    private readonly IMediator _mediator;
    private readonly IPolychatService _ps;

    public RestartNotifierService(ILogger<RestartNotifierService> logger, IMediator mediator, IPolychatService ps) 
        : base(TimeBetweenIterationsInMillis, logger)
    {
        _mediator = mediator;
        _ps = ps;
        _logger = logger;
    }

    protected override async Task OnExecute(CancellationToken ct)
    {
        var jobs = JobStorage.Current
            .GetConnection()
            .GetRecurringJobs()
            .Where(
                j => j.Id.StartsWith("AUTO_RESTART") && 
                     j.NextExecution is not null && 
                     j.NextExecution.Value - DateTime.UtcNow < TimeSpan.FromMinutes(5)
            );

        foreach (var job in jobs)
        {
            var serverId = job.Id[(job.Id.LastIndexOf("_", StringComparison.Ordinal) + 1)..];
            var server = _ps.GetOnlineServerOrDefault(serverId);

            if (server is null)
            {
                _logger.LogWarning("Server {ServerId} scheduled to restart was already offline", serverId);
                return;
            }

            await _mediator.Send(new NotifyAboutRestart.Command(server, job.NextExecution!.Value - DateTime.UtcNow),
                ct);
        }
    }
}