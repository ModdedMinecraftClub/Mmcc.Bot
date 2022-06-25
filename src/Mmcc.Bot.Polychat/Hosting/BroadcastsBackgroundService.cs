using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Polychat.Models.Settings;
using Mmcc.Bot.Polychat.Services;

namespace Mmcc.Bot.Polychat.Hosting;

public class BroadcastsBackgroundService : BackgroundService
{
    private readonly ILogger<BroadcastsBackgroundService> _logger;
    private readonly IPolychatService _ps;
    
    private readonly string? _id;
    private readonly string? _prefix;
    private readonly List<string>? _broadcastMessages;
    
    private PeriodicTimer? _timer;
    private int _broadcastMessagesIndex;
    
    private readonly TimeSpan _periodBetweenIterations = TimeSpan.FromMinutes(7);
    
    public BroadcastsBackgroundService(
        ILogger<BroadcastsBackgroundService> logger,
        IPolychatService ps,
        PolychatSettings polychatSettings
    )
    {
        _logger = logger;
        _ps = ps;

        _id = polychatSettings.BroadcastsSettings?.Id;
        _prefix = polychatSettings.BroadcastsSettings?.Prefix;
        _broadcastMessages = polychatSettings.BroadcastsSettings?.BroadcastMessages;

        _broadcastMessagesIndex = 0;
    }
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting {Service}...", nameof(BroadcastsBackgroundService));
        
        if (_broadcastMessages is null
            || _broadcastMessages.Count == 0
            || _id is null
            || _prefix is null
           )
        {
            _logger.LogWarning("Broadcasts configuration is invalid or not set. Stopping the {Service}...",
                nameof(BroadcastsBackgroundService));
            _logger.LogInformation("Stopped {Service}...", nameof(BroadcastsBackgroundService));

            return;
        }
        
        _timer = new(_periodBetweenIterations);
        
        _logger.LogInformation("Started {Service}...", nameof(BroadcastsBackgroundService));
        
        while (await _timer.WaitForNextTickAsync(ct) && !ct.IsCancellationRequested)
        {
            await OnExecute();
        }
    }
    
    private async ValueTask OnExecute()
    {
        try
        {
            await Broadcast();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error has occurred while broadcasting");
        }
        
        _broadcastMessagesIndex = (_broadcastMessagesIndex + 1) % _broadcastMessages!.Count;
    }

    private async ValueTask Broadcast()
    {
        var msg = _broadcastMessages![_broadcastMessagesIndex];
        var proto = new ChatMessage
        {
            ServerId = _id!,
            Message = $"{_prefix} {msg}",
            MessageOffset = _prefix!.Length - 1
        };
            
        await _ps.BroadcastMessage(proto);
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}