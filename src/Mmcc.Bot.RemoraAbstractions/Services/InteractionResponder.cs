using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Common.Models.Settings;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services;

public interface IInteractionResponder
{
    Task<Result> NotifyDeferredMessageIsComing(
        Snowflake interactionId,
        string interactionToken,
        CancellationToken ct = default
    );
        
    Task<Result> SendFollowup(string interactionToken, params Embed[] embeds);

    Task<Result> SendFollowup(string interactionToken, string msg);
}
    
public class InteractionResponder : IInteractionResponder
{
    private readonly DiscordSettings _discordSettings;
    private readonly IDiscordRestInteractionAPI _interactionApi;

    public InteractionResponder(DiscordSettings discordSettings, IDiscordRestInteractionAPI interactionApi)
    {
        _discordSettings = discordSettings;
        _interactionApi = interactionApi;
    }

    public async Task<Result> NotifyDeferredMessageIsComing(
        Snowflake interactionId,
        string interactionToken,
        CancellationToken ct = default
    ) => await _interactionApi.CreateInteractionResponseAsync(interactionId, interactionToken,
        new InteractionResponse(InteractionCallbackType.DeferredChannelMessageWithSource), ct: ct);
        
    public async Task<Result> SendFollowup(string interactionToken, params Embed[] embeds)
    {
        var res = await _interactionApi.CreateFollowupMessageAsync(
            new(_discordSettings.ApplicationId),
            interactionToken,
            embeds: embeds.ToList()
        );
        return res.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(res);
    }
    
    public async Task<Result> SendFollowup(string interactionToken, string msg)
    {
        var res = await _interactionApi.CreateFollowupMessageAsync(
            new(_discordSettings.ApplicationId),
            interactionToken,
            msg
        );
        return res.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(res);
    }
}