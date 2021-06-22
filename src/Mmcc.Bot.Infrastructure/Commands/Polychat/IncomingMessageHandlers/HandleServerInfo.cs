using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core;
using Mmcc.Bot.Core.Utilities;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;

namespace Mmcc.Bot.Infrastructure.Commands.Polychat.IncomingMessageHandlers
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
                var unformattedId = PolychatStringUtils.SanitiseMcId(onlineServer.ServerId.ToUpperInvariant());
                
                _polychatService.AddOrUpdateOnlineServer(unformattedId, onlineServer);
                _logger.LogInformation("Added server {id} to the list of online servers.", unformattedId);
            }
        }
    }
}