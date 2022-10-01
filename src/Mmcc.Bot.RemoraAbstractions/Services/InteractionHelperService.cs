using System.Linq;
using System.Threading.Tasks;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.RemoraAbstractions.UI.Extensions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services;

public interface IInteractionHelperService
{
    Task<Result> NotifyDeferredMessageIsComing();
    Task<Result> RespondWithModal(InteractionModalCallbackData modalCallbackData);
    Task<Result> SendFollowup(params Embed[] embeds);
    Task<Result> SendFollowup(string msg);
    Task<Result> SendErrorNotification(IResultError err);
}

public class InteractionHelperService : IInteractionHelperService
{
    private readonly InteractionContext _context;
    private readonly IDiscordRestInteractionAPI _interactionApi;
    private readonly DiscordSettings _discordSettings;
    private readonly IErrorProcessingService _errorProcessingService;

    public InteractionHelperService(
        InteractionContext context,
        IDiscordRestInteractionAPI interactionApi,
        DiscordSettings discordSettings,
        IErrorProcessingService errorProcessingService
    )
    {
        _context = context;
        _interactionApi = interactionApi;
        _discordSettings = discordSettings;
        _errorProcessingService = errorProcessingService;
    }

    public async Task<Result> NotifyDeferredMessageIsComing()
        => await _interactionApi.CreateInteractionResponseAsync(_context.ID, _context.Token,
            new InteractionResponse(InteractionCallbackType.DeferredChannelMessageWithSource));

    public async Task<Result> RespondWithModal(InteractionModalCallbackData modalCallbackData)
        => await _interactionApi.CreateInteractionResponseAsync(_context.ID, _context.Token,
            modalCallbackData.GetInteractionResponse());
    
    public async Task<Result> SendFollowup(params Embed[] embeds)
    {
        var res = await _interactionApi.CreateFollowupMessageAsync(
            new(_discordSettings.ApplicationId),
            _context.Token,
            embeds: embeds.ToList()
        );
        return res.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(res);
    }
    
    public async Task<Result> SendFollowup(string msg)
    {
        var res = await _interactionApi.CreateFollowupMessageAsync(
            new(_discordSettings.ApplicationId),
            _context.Token,
            msg
        );
        return res.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(res);
    }

    public async Task<Result> SendErrorNotification(IResultError err)
    {
        var errorEmbed = _errorProcessingService.GetErrorEmbed(err);

        var sendErrorEmbed = await SendFollowup(errorEmbed);
        return !sendErrorEmbed.IsSuccess
            ? Result.FromError(sendErrorEmbed.Error)
            : Result.FromSuccess();
    }
}