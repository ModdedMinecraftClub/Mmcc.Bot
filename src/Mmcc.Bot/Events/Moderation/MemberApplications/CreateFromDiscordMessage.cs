using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Mmcc.Bot.Events.Moderation.MemberApplications
{
    /// <summary>
    /// Creates a member application from a Discord message.
    /// </summary>
    public class CreateFromDiscordMessage
    {
        /// <summary>
        /// Command to create a member application from a Discord message.
        /// </summary>
        public class Command : IRequest<Result>
        {
            /// <summary>
            /// Gateway event sent when the message containing the application was created.
            /// </summary>
            public IMessageCreate DiscordMessageCreatedEvent { get; set; } = null!;
        }

        /// <summary>
        /// Validates the <see cref="Command"/>.
        /// </summary>
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.DiscordMessageCreatedEvent)
                    .NotNull();

                RuleFor(c => c.DiscordMessageCreatedEvent.GuildID.Value)
                    .NotNull();

                RuleFor(c => c.DiscordMessageCreatedEvent.GuildID.Value.Value)
                    .NotNull();

                RuleFor(c => c.DiscordMessageCreatedEvent.ChannelID.Value)
                    .NotNull();

                RuleFor(c => c.DiscordMessageCreatedEvent.ID.Value)
                    .NotNull();

                RuleFor(c => c.DiscordMessageCreatedEvent.Author.ID.Value)
                    .NotNull();

                RuleFor(c => c.DiscordMessageCreatedEvent.Author.Username)
                    .NotEmpty();

                RuleFor(c => c.DiscordMessageCreatedEvent.Author.Discriminator)
                    .NotNull();

                RuleFor(c => c.DiscordMessageCreatedEvent.Timestamp)
                    .NotEmpty();

                RuleFor(c => c.DiscordMessageCreatedEvent.Attachments)
                    .NotEmpty();
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
                    var app = new MemberApplication(
                        guildId: request.DiscordMessageCreatedEvent.GuildID.Value.Value,
                        channelId: request.DiscordMessageCreatedEvent.ChannelID.Value,
                        messageId: request.DiscordMessageCreatedEvent.ID.Value,
                        authorDiscordId: request.DiscordMessageCreatedEvent.Author.ID.Value,
                        authorDiscordName: $"{request.DiscordMessageCreatedEvent.Author.Username}#{request.DiscordMessageCreatedEvent.Author.Discriminator}",
                        appStatus: ApplicationStatus.Pending,
                        appTime: request.DiscordMessageCreatedEvent.Timestamp.ToUnixTimeMilliseconds(),
                        messageContent: request.DiscordMessageCreatedEvent.Content,
                        imageUrl: request.DiscordMessageCreatedEvent.Attachments[0].Url
                    );

                    await _context.MemberApplications.AddAsync(app, cancellationToken);
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