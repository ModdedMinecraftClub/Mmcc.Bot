using System;
using System.Collections.Generic;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.Polychat.MessageSenders
{
    /// <summary>
    /// Sends a command to server(s).
    /// </summary>
    public class SendExecCommand
    {
        /// <summary>
        /// Command to send a command to server(s).
        /// </summary>
        public record Command(string ServerId, Snowflake ChannelId, IEnumerable<string> McCmdArgs) : IRequest<Result>;
        
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
                    var msg = new GenericCommand
                    {
                        DiscordCommandName = "exec",
                        DefaultCommand = "$exec",
                        Args = {request.McCmdArgs},
                        DiscordChannelId = request.ChannelId.ToString()
                    };

                    if (request.ServerId.Equals("<all>"))
                    {
                        _polychatService.BroadcastMessage(msg);
                        return Result.FromSuccess();
                    }

                    var server = _polychatService.GetOnlineServerOrDefault(request.ServerId);

                    if (server is null)
                    {
                        return new NotFoundError($"Could not find server with ID: {request.ServerId}");
                    }

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