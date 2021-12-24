using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Polychat.Abstractions;
using Mmcc.Bot.Polychat.Models.Settings;
using Mmcc.Bot.Polychat.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Mmcc.Bot.Polychat.MessageHandlers;

public class HandlePlayerStatusChangedMessage
{
    public class Handler : AsyncRequestHandler<TcpRequest<ServerPlayerStatusChangedEvent>>
    {
        private readonly IPolychatService _polychatService;
        private readonly PolychatSettings _polychatSettings;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IColourPalette _colourPalette;

        public Handler(IPolychatService polychatService, PolychatSettings polychatSettings, IDiscordRestChannelAPI channelApi, IColourPalette colourPalette)
        {
            _polychatService = polychatService;
            _polychatSettings = polychatSettings;
            _channelApi = channelApi;
            _colourPalette = colourPalette;
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
                
            var newPlayersList = request.Message.NewPlayersOnline.PlayerNames.ToList();

            if (
                request.Message.NewPlayerStatus is ServerPlayerStatusChangedEvent.Types.PlayerStatus.Left
                && newPlayersList.Contains(request.Message.PlayerUsername)
            )
            {
                newPlayersList.Remove(request.Message.PlayerUsername);
            }
            else if (
                request.Message.NewPlayerStatus is ServerPlayerStatusChangedEvent.Types.PlayerStatus.Joined
                && !newPlayersList.Contains(request.Message.PlayerUsername)
            )
            {
                newPlayersList.Add(request.Message.PlayerUsername);
            }

            server.OnlinePlayerNames = newPlayersList;
            server.PlayersOnline = newPlayersList.Count;

            _polychatService.AddOrUpdateOnlineServer(sanitisedId, server);
            await _polychatService.ForwardMessage(sanitisedId, request.Message);

            var messageStr = new PolychatChatMessageString(
                sanitisedId,
                $"{request.Message.PlayerUsername} has {request.Message.NewPlayerStatus.ToString().ToLower()} the game.");
            
            var getChatChannelResult = await _channelApi.GetChannelAsync(new(_polychatSettings.ChatChannelId), cancellationToken);
            
            if (!getChatChannelResult.IsSuccess)
            {
                throw new Exception(getChatChannelResult.Error.Message);
            }
            
            var colour = request.Message.NewPlayerStatus switch
            {
                ServerPlayerStatusChangedEvent.Types.PlayerStatus.Joined => _colourPalette.Green,
                ServerPlayerStatusChangedEvent.Types.PlayerStatus.Left => _colourPalette.Black,
                _ => _colourPalette.Gray
            };
            var embed = new Embed(messageStr.ToSanitisedString(), Colour: colour);
            var sendMessageResult = await _channelApi.CreateMessageAsync(
                new(_polychatSettings.ChatChannelId),
                embeds: new[] {embed},
                ct: cancellationToken);
            
            if (!sendMessageResult.IsSuccess)
            {
                throw new Exception(getChatChannelResult.Error?.Message ?? "Could not forward message to Discord.");
            }
        }
    }
}