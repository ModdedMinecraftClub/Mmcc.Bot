using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Mmcc.Bot.RemoraAbstractions.Services;
using Mmcc.Bot.RemoraAbstractions.Services.MessageResponders;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace Mmcc.Bot.Commands.Core;

/// <summary>
/// Help commands.
/// </summary>
public class HelpCommands : CommandGroup
{
    private readonly CommandMessageResponder _responder;
    private readonly HelpService _helpService;
    
    public HelpCommands(CommandMessageResponder responder, HelpService helpService)
    {
        _responder = responder;
        _helpService = helpService;
    }

    [Command("help")]
    [Description("Shows available commands")]
    public async Task<IResult> Help()
    {
        var helpEmbed = _helpService.GetHelpForAll();

        return await _responder.Respond(helpEmbed);
    }

    [Command("help")]
    [Description("Shows help for a given category")]
    public async Task<IResult> Help([Greedy] IEnumerable<string> path) =>
        _helpService.GetHelpForCategory(path.ToList()) switch
        {
            { IsSuccess: true, Entity: { } embed } =>
                await _responder.Respond(embed),

            { IsSuccess: false } res => res
        };
}