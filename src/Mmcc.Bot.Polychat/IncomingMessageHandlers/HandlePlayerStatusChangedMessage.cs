using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Polychat.Abstractions;
using Mmcc.Bot.Polychat.Models.Settings;
using Remora.Discord.API.Abstractions.Rest;

namespace Mmcc.Bot.Polychat.IncomingMessageHandlers
{
    public class HandlePlayerStatusChangedMessage
    {
        public class Handler : AsyncRequestHandler<TcpRequest<ServerPlayerStatusChangedEvent>>
        {
            private readonly IPolychatService _polychatService;
            private readonly PolychatSettings _polychatSettings;
            private readonly IDiscordRestChannelAPI _channelApi;

            public Handler(IPolychatService polychatService, PolychatSettings polychatSettings, IDiscordRestChannelAPI channelApi)
            {
                _polychatService = polychatService;
                _polychatSettings = polychatSettings;
                _channelApi = channelApi;
            }

            protected override async Task Handle(TcpRequest<ServerPlayerStatusChangedEvent> request, CancellationToken cancellationToken)
            {
                var serverId = new PolychatServerIdString(request.Message.NewPlayersOnline.ServerId);
                var sanitisedId = serverId.ToSanitisedUppercase();
                var server = _polychatService.GetOnlineServerOrDefault(sanitisedId);

                if (server is null)
                {
                    throw new KeyNotFoundException($"Could not find server {sanitisedId} in the list of online servers");
                }

                var playersOnline = request.Message.NewPlayersOnline.PlayersOnline;
                var newPlayersList = request.Message.NewPlayersOnline.PlayerNames.ToList();

                if (request.Message.NewPlayerStatus is ServerPlayerStatusChangedEvent.Types.PlayerStatus.Left
                    && newPlayersList.Contains(request.Message.PlayerUsername)
                )
                {
                    newPlayersList.Remove(request.Message.PlayerUsername);
                    playersOnline = playersOnline == 0 ? playersOnline : playersOnline - 1;
                }
                else if (request.Message.NewPlayerStatus is ServerPlayerStatusChangedEvent.Types.PlayerStatus.Joined
                         && !newPlayersList.Contains(request.Message.PlayerUsername)
                )
                {
                    newPlayersList.Add(request.Message.PlayerUsername);
                    playersOnline += 1;
                }

                server.PlayersOnline = playersOnline;
                server.OnlinePlayerNames = newPlayersList;

                _polychatService.AddOrUpdateOnlineServer(sanitisedId, server);
                await _polychatService.ForwardMessage(sanitisedId, request.Message);

                var messageStr = new PolychatChatMessageString(
                    sanitisedId,
                    $"{request.Message.PlayerUsername} has {request.Message.NewPlayerStatus.ToString().ToLower()} the game.");
                var getChatChannelResult =
                    await _channelApi.GetChannelAsync(new(_polychatSettings.ChatChannelId), cancellationToken);
                
                if (!getChatChannelResult.IsSuccess)
                {
                    throw new Exception(getChatChannelResult.Error.Message);
                }
                
                var sendMessageResult = await _channelApi.CreateMessageAsync(
                    new(_polychatSettings.ChatChannelId),
                    messageStr.ToDiscordFormattedString(),
                    ct: cancellationToken);
                    
                if (!sendMessageResult.IsSuccess)
                {
                    throw new Exception(getChatChannelResult.Error?.Message ?? "Could not forward message to Discord.");
                }
            }
        }
    }
}