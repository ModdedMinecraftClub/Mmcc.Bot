using System.ComponentModel;
using System.Threading.Tasks;
using Porbeagle;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace Mmcc.Bot.Commands.MmccInfo;

public class MmccInfoCommands : CommandGroup
{
    private readonly IContextAwareViewManager _viewManager;

    public MmccInfoCommands(IContextAwareViewManager viewManager) =>
        _viewManager = viewManager;

    [Command("mmcc")]
    [Description("Shows useful MMCC links")]
    public async Task<IResult> Mmcc()
        => await _viewManager.RespondWithView(new MmccInfoView());
}