using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.MemberApplications
{
    /// <summary>
    /// Updates a member application from a Discord message.
    /// </summary>
    public class UpdateFromDiscordMessage
    {
        /// <summary>
        /// Command to update a member application from a Discord message.
        /// </summary>
        public class Command : IRequest<Result>
        {
            /// <summary>
            /// Gateway event sent when the message containing the application was updated.
            /// </summary>
            public IMessageUpdate DiscordMessageUpdatedEvent { get; set; } = null!;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.DiscordMessageUpdatedEvent)
                    .NotNull();
            }
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly BotContext _context;
            
            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/>.
            /// </summary>
            /// <param name="context">The db context.</param>
            public Handler(BotContext context)
            {
                _context = context;
            }

            /// <inheritdoc />
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var app = await _context.MemberApplications
                        .Where(a => a.GuildId == request.DiscordMessageUpdatedEvent.GuildID.Value.Value)
                        .FirstOrDefaultAsync(a => a.MessageId == request.DiscordMessageUpdatedEvent.ID.Value.Value,
                            cancellationToken);

                    if (app is null)
                    {
                        return new NotFoundError("Application corresponding to the edited message could not be found.");
                    }

                    if (app.AppStatus != ApplicationStatus.Pending)
                    {
                        return Result.FromSuccess();
                    }
                    
                    app.MessageContent = !request.DiscordMessageUpdatedEvent.Content.HasValue
                        ? null
                        : request.DiscordMessageUpdatedEvent.Content.Value;

                    await _context.SaveChangesAsync(cancellationToken);
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