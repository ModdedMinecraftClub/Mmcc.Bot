using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mmcc.Bot.Core.Models.Settings;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions
{
    public interface IInteractionResponder
    {
        Task<Result> RespondAsynchronously(
            Snowflake interactionId,
            string interactionToken,
            Func<ValueTask<Result<IEnumerable<Embed>>>> embedsProducer
        );
    }
    
    public class InteractionResponder : IInteractionResponder
    {
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly IDiscordRestWebhookAPI _webhookApi;
        private readonly DiscordSettings _discordSettings;

        public InteractionResponder(IDiscordRestInteractionAPI interactionApi, IDiscordRestWebhookAPI webhookApi, DiscordSettings discordSettings)
        {
            _interactionApi = interactionApi;
            _webhookApi = webhookApi;
            _discordSettings = discordSettings;
        }

        public async Task<Result> RespondAsynchronously(
            Snowflake interactionId,
            string interactionToken,
            Func<ValueTask<Result<IEnumerable<Embed>>>> embedsProducer
        )
        {
            var createInteractionRes = await _interactionApi.CreateInteractionResponseAsync(
                interactionId,
                interactionToken,
                new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage)
            );
            if (!createInteractionRes.IsSuccess)
            {
                return createInteractionRes;
            }

            var getEmbedsRes = await embedsProducer();
            if (!getEmbedsRes.IsSuccess)
            {
                return Result.FromError(getEmbedsRes);
            }

            var embeds = getEmbedsRes.Entity;
            var res = await _webhookApi.CreateFollowupMessageAsync(
                new(_discordSettings.ApplicationId),
                interactionToken,
                embeds: embeds.ToList()
            );
            return res.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(res);
        }
    }
}