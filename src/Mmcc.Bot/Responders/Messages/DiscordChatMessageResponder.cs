using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Core;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Core.Utilities;
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

        public DiscordChatMessageResponder(IPolychatService polychatService, PolychatSettings polychatSettings, DiscordSettings discordSettings)
        {
            _polychatService = polychatService;
            _polychatSettings = polychatSettings;
            _discordSettings = discordSettings;
        }

        public Task<Result> RespondAsync(IMessageCreate ev, CancellationToken ct)
        {
            if (ev.Author.IsBot.HasValue && ev.Author.IsBot.Value
                || ev.Author.IsSystem.HasValue && ev.Author.IsSystem.Value
                || !ev.GuildID.HasValue
                || ev.ChannelID.Value != _polychatSettings.ChatChannelId
                || ev.Content.StartsWith(_discordSettings.Prefix)
            )
            {
                return Task.FromResult(Result.FromSuccess());
            }
            
            var protoMsgContent = ev.MentionedChannels.HasValue
                ? $"§9[Discord] §7{ev.Author.Username}§r: {DiscordSanitiser.Sanitise(ev.Content, ev.Mentions, ev.MentionedChannels.Value)}"
                : $"§9[Discord] §7{ev.Author.Username}§r: {DiscordSanitiser.Sanitise(ev.Content, ev.Mentions)}";
            var protoMsg = new ChatMessage
            {
                ServerId = "Discord",
                Message = protoMsgContent,
                MessageOffset = protoMsgContent.IndexOf(':')
            };
            
            _polychatService.BroadcastMessage(protoMsg);
            return Task.FromResult(Result.FromSuccess());
        }
    }
}