using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Mmcc.Bot.Polychat.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.Polychat.MessageSenders;

/// <summary>
/// Sends a command to server(s).
/// </summary>
public class SendExecCommand
{
    /// <summary>
    /// Command to send a command to server(s).
    /// </summary>
    public record Command(string ServerId, Snowflake ChannelId, IEnumerable<string> McCmdArgs) : IRequest<Result>;

    /// <summary>
    /// Validates the <see cref="Command"/>.
    /// </summary>
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ServerId)
                .NotEmpty()
                .MinimumLength(2);

            RuleFor(c => c.ChannelId)
                .NotNull();
        }
    }
        
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IPolychatService _polychatService;

        public Handler(IPolychatService polychatService)
        {
            _polychatService = polychatService;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var msg = new GenericCommand
                {
                    DiscordCommandName = "exec",
                    DefaultCommand = "$args",
                    Args = {request.McCmdArgs},
                    DiscordChannelId = request.ChannelId.ToString()
                };

                if (request.ServerId.Equals("<all>"))
                {
                    await _polychatService.BroadcastMessage(msg);
                    return Result.FromSuccess();
                }

                var server = _polychatService.GetOnlineServerOrDefault(request.ServerId.ToUpperInvariant());

                if (server is null)
                {
                    return new NotFoundError($"Could not find server with ID: {request.ServerId}");
                }

                await _polychatService.SendMessage(server, msg);
                return Result.FromSuccess();
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}