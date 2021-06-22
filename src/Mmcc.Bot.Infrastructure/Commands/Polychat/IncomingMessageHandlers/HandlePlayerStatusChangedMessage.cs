using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Core.Utilities;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;
using Remora.Discord.API.Abstractions.Rest;

namespace Mmcc.Bot.Infrastructure.Commands.Polychat.IncomingMessageHandlers
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
                var serverId = request.Message.NewPlayersOnline.ServerId.ToUpperInvariant();
                var unformattedId = PolychatStringUtils.SanitiseMcId(serverId);
                var server = _polychatService.GetOnlineServerOrDefault(unformattedId);

                if (server is null)
                {
                    throw new KeyNotFoundException($"Could not find server {unformattedId} in the list of online servers");
                }

                server.PlayersOnline = request.Message.NewPlayersOnline.PlayersOnline;
                server.OnlinePlayerNames = request.Message.NewPlayersOnline.PlayerNames.ToList();

                _polychatService.AddOrUpdateOnlineServer(unformattedId, server);
                _polychatService.ForwardMessage(unformattedId, request.Message);

                var messageStr = new PolychatChatMessageString(
                    unformattedId,
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