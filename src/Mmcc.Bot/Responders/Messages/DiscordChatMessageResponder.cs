using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.Responders.Messages
{
    public class DiscordChatMessageResponder : IResponder<IMessageCreate>
    {
        private readonly IPolychatService _polychatService;
        private readonly PolychatSettings _polychatSettings;
        private readonly DiscordSettings _discordSettings;
        private readonly IDiscordSanitiserService _sanitiser;

        public DiscordChatMessageResponder(
            IPolychatService polychatService,
            PolychatSettings polychatSettings,
            DiscordSettings discordSettings,
            IDiscordSanitiserService sanitiser
        )
        {
            _polychatService = polychatService;
            _polychatSettings = polychatSettings;
            _discordSettings = discordSettings;
            _sanitiser = sanitiser;
        }

        public async Task<Result> RespondAsync(IMessageCreate ev, CancellationToken ct)
        {
            if (ev.Author.IsBot.HasValue && ev.Author.IsBot.Value
                || ev.Author.IsSystem.HasValue && ev.Author.IsSystem.Value
                || !ev.GuildID.HasValue
                || ev.ChannelID.Value != _polychatSettings.ChatChannelId
                || ev.Content.StartsWith(_discordSettings.Prefix)
            )
            {
                return Result.FromSuccess();
            }

            var sanitisedMsgContent = await _sanitiser.SanitiseMessageContent(ev);
            var protoMsgContent = $"§9[Discord] §7{ev.Author.Username}§r: {sanitisedMsgContent}";
            var protoMsg = new ChatMessage
            {
                ServerId = "Discord",
                Message = protoMsgContent,
                MessageOffset = protoMsgContent.IndexOf(':')
            };

            await _polychatService.BroadcastMessage(protoMsg);
            return Result.FromSuccess();
        }
    }
}