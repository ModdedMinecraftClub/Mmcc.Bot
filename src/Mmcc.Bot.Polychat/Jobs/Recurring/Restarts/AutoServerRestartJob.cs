using System;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Polychat.MessageSenders;
using Mmcc.Bot.Polychat.Models.Settings;
using Mmcc.Bot.Polychat.Services;

namespace Mmcc.Bot.Polychat.Jobs.Recurring.Restarts;

[Queue("serverrestarts")]
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 90 })]
public class AutoServerRestartJob
{
    private readonly ILogger<AutoServerRestartJob> _logger;
    private readonly PolychatSettings _settings;
    private readonly IPolychatService _polychatService;
    private readonly IMediator _mediator;

    public static string CreateJobId(string serverId) => $"{PolychatJobIdPrefixes.Restart}_{serverId}";

    public AutoServerRestartJob(
        ILogger<AutoServerRestartJob> logger,
        PolychatSettings settings,
        IPolychatService polychatService,
        IMediator mediator
    )
    {
        _logger = logger;
        _settings = settings;
        _polychatService = polychatService;
        _mediator = mediator;
    }

    public async Task Execute(string serverId)
    {
        var server = _polychatService.GetOnlineServerOrDefault(serverId);

        if (server is null)
        {
            _logger.LogWarning("Server {ServerId} scheduled to restart was already offline", serverId);
            return;
        }

        for (var i = 5; i > 0; i--)
        {
            await _mediator.Send(new Notify.Command(server, TimeSpan.FromSeconds(i)));
            await Task.Delay(1000);
        }

        await _mediator.Send(new SendRestartCommand.Command(serverId, new(_settings.ChatChannelId)));
    }
}