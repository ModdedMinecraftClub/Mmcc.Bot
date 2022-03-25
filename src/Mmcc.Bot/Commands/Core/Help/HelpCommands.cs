using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.Commands.Core.Help;

/// <summary>
/// Help commands.
/// </summary>
public class HelpCommands : CommandGroup
{
    private readonly MessageContext _context;
    private readonly ICommandResponder _responder;
    private readonly IDmSender _dmSender;
    private readonly IMediator _mediator;

    /// <summary>
    /// Instantiates a new instance of <see cref="HelpCommands"/> class.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="responder">The command responder.</param>
    /// <param name="dmSender">The DM sender.</param>
    /// <param name="mediator">The mediator.</param>
    public HelpCommands(
        MessageContext context,
        ICommandResponder responder,
        IDmSender dmSender,
        IMediator mediator 
    )
    {
        _context = context;
        _responder = responder;
        _mediator = mediator;
        _dmSender = dmSender;
    }

    [Command("help")]
    [Description("Shows available commands")]
    public async Task<IResult> Help()
    {
        var getEmbedsResult = await _mediator.Send(new GetForAll.Query());
        
        if (!getEmbedsResult.IsSuccess)
        {
            return getEmbedsResult;
        }

        var embedChunks = getEmbedsResult.Entity.Chunk(10);

        foreach (var embeds in embedChunks)
        {
            var sendDmChunkRes = await _dmSender.Send(_context.User.ID, embeds);
            
            if (!sendDmChunkRes.IsSuccess)
            {
                return sendDmChunkRes;
            }
        }
        
        return await _responder.Respond("Help has been sent to your DMs :smile:.");
    }

    [Command("help")]
    [Description("Shows help for a given category")]
    public async Task<IResult> Help(string categoryName) =>
        await _mediator.Send(new GetHelpForCategory.Query(categoryName)) switch
        {
            { IsSuccess: true, Entity: { } embed } =>
                await _responder.Respond(embed),

            { IsSuccess: true } =>
                Result.FromError(new NotFoundError($"Could not find a category with name `{categoryName}`.")),

            { IsSuccess: false } res => res
        };
}