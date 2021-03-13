using System;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.Polychat
{
    /// <summary>
    /// Sends a restart command to a server.
    /// </summary>
    public class SendRestartCommand
    {
        /// <summary>
        /// Command to send a restart command to a server.
        /// </summary>
        public record Command(string ServerId, Snowflake ChannelId) : IRequest<Result>;

        public class Handler : RequestHandler<Command, Result>
        {
            private readonly IPolychatService _polychatService;

            public Handler(IPolychatService polychatService)
            {
                _polychatService = polychatService;
            }
            
            protected override Result Handle(Command request)
            {
                try
                {
                    var server = _polychatService.GetOnlineServerOrDefault(request.ServerId);

                    if (server is null)
                    {
                        return new NotFoundError($"Could not find server with ID: {request.ServerId}");
                    }

                    var msg = new GenericCommand
                    {
                        DiscordCommandName = "restart",
                        DefaultCommand = "stop",
                        DiscordChannelId = request.ChannelId.ToString()
                    };

                    _polychatService.SendMessage(server, msg);
                    return Result.FromSuccess();
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}