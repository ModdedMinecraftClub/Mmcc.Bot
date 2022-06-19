using System.Collections.Generic;
using System.Linq;
using MediatR;
using Mmcc.Bot.Polychat.Abstractions;
using Mmcc.Bot.Polychat.Networking;
using Mmcc.Bot.Polychat.Services;

namespace Mmcc.Bot.Polychat.MessageHandlers;

public class HandlePlayersOnlineMessage
{
    public class Handler : RequestHandler<PolychatRequest<ServerPlayersOnline>>
    {
        private readonly IPolychatService _polychatService;

        public Handler(IPolychatService polychatService)
        {
            _polychatService = polychatService;
        }

        protected override void Handle(PolychatRequest<ServerPlayersOnline> request)
        {
            var serverId = new PolychatServerIdString(request.Message.ServerId);
            var sanitisedId = serverId.ToSanitisedUppercase();
            var server = _polychatService.GetOnlineServerOrDefault(sanitisedId);

            if (server is null)
            {
                throw new KeyNotFoundException($"Could not find server {sanitisedId} in the list of online servers");
            }

            server.PlayersOnline = request.Message.PlayersOnline;
            server.OnlinePlayerNames = request.Message.PlayerNames.ToList();

            _polychatService.AddOrUpdateOnlineServer(sanitisedId, server);
        }
    }
}