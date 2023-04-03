using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Features.Guilds.Views;
using Mmcc.Bot.RemoraAbstractions.Conditions.CommandSpecific;
using Mmcc.Bot.RemoraAbstractions.Services.MessageResponders;
using Porbeagle;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.Features.Guilds;

[RequireGuild]
public class GuildCommands : CommandGroup
{
    private readonly MessageContext _context;
    private readonly IColourPalette _colourPalette;
    private readonly IMediator _mediator;
    private readonly CommandMessageResponder _responder;
    private readonly IContextAwareViewManager _viewManager;
    
    public GuildCommands(
        MessageContext context,
        IColourPalette colourPalette,
        IMediator mediator,
        CommandMessageResponder responder, 
        IContextAwareViewManager viewManager
    )
    {
        _context = context;
        _colourPalette = colourPalette;
        _mediator = mediator;
        _responder = responder;
        _viewManager = viewManager;
    }

    [Command("guild")]
    [Description("Provides information about the current guild.")]
    public async Task<IResult> GuildInfo() =>
        await _mediator.Send(new GetGuildInfo.Query(_context.GuildID.Value)) switch
        {
            { IsSuccess: true, Entity: { } guildInfo } =>
                await _viewManager.RespondWithView(new GuildInfoView(guildInfo)),

            { IsSuccess: true } =>
                Result.FromError(new NotFoundError($"Guild with ID: {_context.GuildID.Value} not found")),

            { IsSuccess: false } res => res
        };

    [Command("invite")]
    [Description("Gives an invite link to the current guild.")]
    public async Task<IResult> Invite() =>
        await _mediator.Send(new GetInviteLink.Query(_context.GuildID.Value)) switch
        {
            {IsSuccess: true, Entity: { } e} =>
                await _responder.Respond($"https://discord.gg/{e}"),

            {IsSuccess: true} => Result.FromError(new NotFoundError("Could not find invite link for this guild.")),

            {IsSuccess: false} res => res
        };
}