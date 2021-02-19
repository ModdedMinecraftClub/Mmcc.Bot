using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.MemberApplications
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
                        authorDiscordName: $"{request.DiscordMessageCreatedEvent.Author.Username}${request.DiscordMessageCreatedEvent.Author.Discriminator}",
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