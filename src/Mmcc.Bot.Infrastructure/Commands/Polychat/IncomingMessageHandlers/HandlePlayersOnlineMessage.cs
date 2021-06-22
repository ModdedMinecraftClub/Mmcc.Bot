using System.Collections.Generic;
using System.Linq;
using MediatR;
using Mmcc.Bot.Core.Utilities;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;

namespace Mmcc.Bot.Infrastructure.Commands.Polychat.IncomingMessageHandlers
{
    public class HandlePlayersOnlineMessage
    {
        public class Handler : RequestHandler<TcpRequest<ServerPlayersOnline>>
        {
            private readonly IPolychatService _polychatService;

            public Handler(IPolychatService polychatService)
            {
                _polychatService = polychatService;
            }

            protected override void Handle(TcpRequest<ServerPlayersOnline> request)
            {
                var serverId = request.Message.ServerId.ToUpperInvariant();
                var unformattedId = PolychatStringUtils.SanitiseMcId(serverId);
                var server = _polychatService.GetOnlineServerOrDefault(unformattedId);

                if (server is null)
                {
                    throw new KeyNotFoundException($"Could not find server {unformattedId} in the list of online servers");
                }

                server.PlayersOnline = request.Message.PlayersOnline;
                server.OnlinePlayerNames = request.Message.PlayerNames.ToList();

                _polychatService.AddOrUpdateOnlineServer(unformattedId, server);
            }
        }
    }
}