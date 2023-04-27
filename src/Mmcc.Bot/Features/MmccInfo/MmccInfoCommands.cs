using System.ComponentModel;
using System.Threading.Tasks;
using Mmcc.Bot.Commands.MmccInfo;
using Porbeagle;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace Mmcc.Bot.Features.MmccInfo;

public class MmccInfoCommands : CommandGroup
{
    private readonly IContextAwareViewManager _viewManager;

    public MmccInfoCommands(IContextAwareViewManager viewManager) =>
        _viewManager = viewManager;

    [Command("mmcc")]
    [Description("Shows useful MMCC links")]
    public async Task<IResult> GetMmccInfo()
        => await _viewManager.RespondWithView(new MmccInfoView());
}