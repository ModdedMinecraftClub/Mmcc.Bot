using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.RemoraAbstractions.Conditions.CommandSpecific;
using Porbeagle;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

namespace Mmcc.Bot.Features.Diagnostics;

[Group("diagnostics")]
[Description("Server and bot diagnostics")]
public sealed class DiagnosticsCommands : CommandGroup
{
    private readonly IMediator _mediator;
    private readonly IContextAwareViewManager _viewManager;

    public DiagnosticsCommands(IMediator mediator, IContextAwareViewManager viewManager)
    {
        _mediator = mediator;
        _viewManager = viewManager;
    }

    [Command("bot")]
    [Description("Show status of the bot and APIs it uses")]
    public async Task<IResult> BotDiagnostics()
    {
        var result = await _mediator.Send(new GetBotDiagnostics.Query());

        return result switch
        {
            { IsSuccess: true, Entity: {  } pingResults } 
                => await _viewManager.RespondWithView(new GetBotDiagnosticsView(pingResults)),
            
            { IsSuccess: false } => result
        };
    }
    
    [Command("drives")]
    [Description("Shows drives info (including free space)")]
    [RequireGuild]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public async Task<IResult> DrivesDiagnostics()
    {
        var result = await _mediator.Send(new GetDrivesDiagnostics.Query());

        return await _viewManager.RespondWithView(new GetDrivesDiagnosticsView(result));
    }
}