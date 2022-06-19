using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Polychat.Abstractions;
using Mmcc.Bot.Polychat.Models.Settings;
using Mmcc.Bot.Polychat.Networking;
using Mmcc.Bot.Polychat.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.Polychat.MessageHandlers;

/// <summary>
/// Handles an incoming <see cref="ServerStatus"/> message.
/// </summary>
public class HandleServerStatus
{
    public class Handler : AsyncRequestHandler<PolychatRequest<ServerStatus>>
    {
        private readonly IPolychatService _polychatService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly PolychatSettings _polychatSettings;
        private readonly ILogger<HandleServerStatus> _logger;
        private readonly IColourPalette _colourPalette;

        public Handler(
            IPolychatService polychatService,
            IDiscordRestChannelAPI channelApi,
            PolychatSettings polychatSettings,
            ILogger<HandleServerStatus> logger,
            IColourPalette colourPalette
        )
        {
            _polychatService = polychatService;
            _channelApi = channelApi;
            _polychatSettings = polychatSettings;
            _logger = logger;
            _colourPalette = colourPalette;
        }

        protected override async Task Handle(PolychatRequest<ServerStatus> request, CancellationToken cancellationToken)
        {
            var msg = request.Message;
            var serverId = new PolychatServerIdString(msg.ServerId);
            var sanitisedId = serverId.ToSanitisedUppercase();
            var server = _polychatService.GetOnlineServerOrDefault(sanitisedId);

            if (server is not null)
            {
                await _polychatService.ForwardMessage(sanitisedId, msg);

                if (
                    msg.Status is
                        ServerStatus.Types.ServerStatusEnum.Stopped or ServerStatus.Types.ServerStatusEnum.Crashed
                )
                {
                    _polychatService.RemoveOnlineServer(sanitisedId);
                    _logger.LogInformation("Removed server {id} from the list of online servers.", sanitisedId);
                    request.ConnectedClient.StopListening();
                }

                var getChatChannelResult =
                    await _channelApi.GetChannelAsync(new(_polychatSettings.ChatChannelId), cancellationToken);
                if (!getChatChannelResult.IsSuccess)
                {
                    throw new Exception(getChatChannelResult.Error.Message);
                }

                Optional<Color> embedColour = msg.Status switch
                {
                    ServerStatus.Types.ServerStatusEnum.Started => _colourPalette.Green,
                    ServerStatus.Types.ServerStatusEnum.Crashed => _colourPalette.Red,
                    ServerStatus.Types.ServerStatusEnum.Stopped => _colourPalette.Black,
                    _ => new()
                };
                var message = $"Server {msg.Status.ToString().ToLowerInvariant()}.";
                var chatMessage = new PolychatChatMessageString(sanitisedId, message);
                var embed = new Embed(chatMessage.ToSanitisedString(), Colour: embedColour);
                var sendMessageResult = await _channelApi.CreateMessageAsync(new(_polychatSettings.ChatChannelId),
                    embeds: new[] {embed}, ct: cancellationToken);
                
                if (!sendMessageResult.IsSuccess)
                {
                    throw new Exception(getChatChannelResult.Error?.Message ?? "Could not forward message to Discord.");
                }

                _logger.LogInformation("Server {id} changed status to {newStatus}", sanitisedId,
                    msg.Status);
            }
            else
            {
                if (msg.Status == ServerStatus.Types.ServerStatusEnum.Started)
                {
                    _logger.LogWarning(
                        "Server {id} has unexpectedly sent ServerStatus message before sending ServerInfo message.",
                        sanitisedId);
                }
                else
                {
                    throw new KeyNotFoundException($"Could not find server {sanitisedId} in the list of online servers");
                }
            }
        }
    }
}