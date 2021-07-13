using System.Linq;
using System.Threading.Tasks;
using Mmcc.Bot.Core.Models.Settings;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions
{
    public interface IInteractionResponder
    {
        Task<Result> SendFollowup(string interactionToken, params Embed[] embeds);
    }
    
    public class InteractionResponder : IInteractionResponder
    {
        private readonly IDiscordRestWebhookAPI _webhookApi;
        private readonly DiscordSettings _discordSettings;

        public InteractionResponder(IDiscordRestWebhookAPI webhookApi, DiscordSettings discordSettings)
        {
            _webhookApi = webhookApi;
            _discordSettings = discordSettings;
        }

        public async Task<Result> SendFollowup(string interactionToken, params Embed[] embeds)
        {
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