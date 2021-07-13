using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Polychat.MessageSenders
{
    /// <summary>
    /// Sends a TPS command to a server.
    /// </summary>
    public class SendTpsCommand
    {
        /// <summary>
        /// Command to send a TPS command to a server.
        /// </summary>
        public record Command(string ServerId, Snowflake ChannelId) : IRequest<Result>;

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
                    var id = request.ServerId.ToUpperInvariant();
                    var server = _polychatService.GetOnlineServerOrDefault(id);

                    if (server is null)
                    {
                        return new NotFoundError($"Could not find server with ID: {id}");
                    }

                    var msg = new GenericCommand
                    {
                        DiscordCommandName = "tps",
                        DefaultCommand = "forge tps",
                        DiscordChannelId = request.ChannelId.ToString()
                    };

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
}