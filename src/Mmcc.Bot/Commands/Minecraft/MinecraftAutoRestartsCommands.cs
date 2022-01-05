using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Polychat.Jobs.Recurring;
using Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.Commands.Minecraft;

[Group("mcrestarts", "mcr", "restarts")]
[Description("Commands for managing automatic restarts.")]
[RequireGuild]
public class MinecraftAutoRestartsCommands : CommandGroup
{
    private readonly MessageContext _context;
    private readonly IMediator _mediator;
    private readonly IColourPalette _colourPalette;
    private readonly ICommandResponder _responder;

    public MinecraftAutoRestartsCommands(
        MessageContext context,
        IMediator mediator,
        IColourPalette colourPalette,
        ICommandResponder responder
    )
    {
        _context = context;
        _mediator = mediator;
        _colourPalette = colourPalette;
        _responder = responder;
    }

    [Command("new")]
    [Description("Creates a new recurring restart")]
    public async Task<IResult> New(string serverId, string cronExpression)
    {
        try
        {
            RecurringJob.AddOrUpdate<AutoServerRestartJob>(AutoServerRestartJob.CreateJobId(serverId),
                job => job.Execute(serverId), Cron.Minutely);
        }
        catch(Exception e)
        {
            return Result.FromError(new ExceptionError(e));
        }

        return await _responder.Respond("Done");
    }
}