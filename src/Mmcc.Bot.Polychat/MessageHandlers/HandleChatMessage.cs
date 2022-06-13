using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Polychat.Abstractions;
using Mmcc.Bot.Polychat.Models.Settings;
using Mmcc.Bot.Polychat.Networking;
using Mmcc.Bot.Polychat.Services;
using Remora.Discord.API.Abstractions.Rest;

namespace Mmcc.Bot.Polychat.MessageHandlers;

public class HandleChatMessage
{
    public class Handler : AsyncRequestHandler<TcpRequest<ChatMessage>>
    {
        private readonly IPolychatService _polychatService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly PolychatSettings _polychatSettings;

        public Handler(IPolychatService polychatService, IDiscordRestChannelAPI channelApi, PolychatSettings polychatSettings)
        {
            _polychatService = polychatService;
            _channelApi = channelApi;
            _polychatSettings = polychatSettings;
        }
            
        protected override async Task Handle(TcpRequest<ChatMessage> request, CancellationToken cancellationToken)
        {
            var protoMessage = request.Message;
            var id = new PolychatServerIdString(protoMessage.ServerId);
            var sanitisedId = id.ToSanitisedUppercase();
            await _polychatService.ForwardMessage(sanitisedId, protoMessage);
                
            var getChatChannelResult =
                await _channelApi.GetChannelAsync(new(_polychatSettings.ChatChannelId), cancellationToken);
                
            if (!getChatChannelResult.IsSuccess)
            {
                throw new Exception(getChatChannelResult.Error.Message);
            }

            var messageTextContent = protoMessage.Message;
            var sanitisedString = new PolychatChatMessageString(messageTextContent).ToSanitisedString();
            var discordMessageInfo = sanitisedString[..protoMessage.MessageOffset];
            var discordMessageContent = sanitisedString[protoMessage.MessageOffset..];
            var fullDiscordMessageStr = $"`{discordMessageInfo}` {discordMessageContent}";
            var sendMessageResult = await _channelApi.CreateMessageAsync(
                new(_polychatSettings.ChatChannelId),
                fullDiscordMessageStr,
                ct: cancellationToken);
                    
            if (!sendMessageResult.IsSuccess)
            {
                throw new Exception(getChatChannelResult.Error?.Message ?? "Could not forward message to Discord.");
            }
        }
    }
}