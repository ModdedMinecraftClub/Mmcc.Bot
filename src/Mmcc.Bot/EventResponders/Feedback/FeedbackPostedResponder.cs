using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Common.Models.Settings;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.EventResponders.Feedback;

public class FeedbackPostedResponder : IResponder<IMessageCreate>
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly DiscordSettings _discordSettings;

    public FeedbackPostedResponder(IDiscordRestChannelAPI channelApi, DiscordSettings discordSettings)
    {
        _channelApi = channelApi;
        _discordSettings = discordSettings;
    }

    public async Task<Result> RespondAsync(IMessageCreate ev, CancellationToken ct = default)
    {
        if (ev.Author.IsBot.HasValue && ev.Author.IsBot.Value
            || ev.Author.IsSystem.HasValue && ev.Author.IsSystem.Value
            || !ev.GuildID.HasValue
            || ev.ChannelID.Value != _discordSettings.FeedbackChannelId
        )
        {
            return Result.FromSuccess();
        }

        var createUpReactionResult = await _channelApi.CreateReactionAsync(ev.ChannelID, ev.ID, "👍", ct);
        if (!createUpReactionResult.IsSuccess)
        {
            return createUpReactionResult;
        }

        return await _channelApi.CreateReactionAsync(ev.ChannelID, ev.ID, "👎", ct);
    }
}