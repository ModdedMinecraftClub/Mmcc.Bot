using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Polychat.Abstractions;
using Mmcc.Bot.Polychat.Models;
using Mmcc.Bot.Polychat.Services;

namespace Mmcc.Bot.Polychat.MessageHandlers
{
    /// <summary>
    /// Handles an incoming <see cref="ServerInfo"/> message.
    /// </summary>
    public class HandleServerInfo
    {
        public class Handler : RequestHandler<TcpRequest<ServerInfo>>
        {
            private readonly ILogger<HandleServerInfo> _logger;
            private readonly IPolychatService _polychatService;

            public Handler(ILogger<HandleServerInfo> logger, IPolychatService polychatService)
            {
                _logger = logger;
                _polychatService = polychatService;
            }

            protected override void Handle(TcpRequest<ServerInfo> request)
            {
                var onlineServer = new OnlineServer(request.Message, request.ConnectedClient);
                var id = new PolychatServerIdString(request.Message.ServerId);
                var sanitisedId = id.ToSanitisedUppercase();
                
                _logger.LogInformation("Adding server {id} to the list of online servers...", sanitisedId);
                _polychatService.AddOrUpdateOnlineServer(sanitisedId, onlineServer);
                _logger.LogInformation("Added server {id} to the list of online servers.", sanitisedId);
            }
        }
    }
}