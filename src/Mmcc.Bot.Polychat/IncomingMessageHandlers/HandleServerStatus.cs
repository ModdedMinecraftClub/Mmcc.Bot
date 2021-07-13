using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Polychat.Abstractions;
using Mmcc.Bot.Polychat.Models.Settings;
using Remora.Discord.API.Abstractions.Rest;

namespace Mmcc.Bot.Polychat.IncomingMessageHandlers
{
    /// <summary>
    /// Handles an incoming <see cref="ServerStatus"/> message.
    /// </summary>
    public class HandleServerStatus
    {
        public class Handler : AsyncRequestHandler<TcpRequest<ServerStatus>>
        {
            private readonly IPolychatService _polychatService;
            private readonly IDiscordRestChannelAPI _channelApi;
            private readonly PolychatSettings _polychatSettings;
            private readonly ILogger<HandleServerStatus> _logger;

            public Handler(IPolychatService polychatService, IDiscordRestChannelAPI channelApi, PolychatSettings polychatSettings, ILogger<HandleServerStatus> logger)
            {
                _polychatService = polychatService;
                _channelApi = channelApi;
                _polychatSettings = polychatSettings;
                _logger = logger;
            }

            protected override async Task Handle(TcpRequest<ServerStatus> request, CancellationToken cancellationToken)
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

                    var message = $"Server {msg.Status.ToString().ToLowerInvariant()}.";
                    var chatMessage = new PolychatChatMessageString(sanitisedId, message);
                    var sendMessageResult =
                        await _channelApi.CreateMessageAsync(new(_polychatSettings.ChatChannelId),
                            chatMessage.ToDiscordFormattedString(), ct: cancellationToken);
                    if (!sendMessageResult.IsSuccess)
                    {
                        throw new Exception(getChatChannelResult.Error?.Message ??
                                            "Could not forward message to Discord.");
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
}