using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.Tags
{
    /// <summary>
    /// Creates a new tag.
    /// </summary>
    public class Create
    {
        /// <summary>
        /// Command to create a new tag.
        /// </summary>
        public record Command
            (Snowflake GuildId, Snowflake Author, string TagName, string? Description, string Content) : IRequest<Result<Tag>>;
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Command, Result<Tag>>
        {
            private readonly BotContext _context;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/>.
            /// </summary>
            /// <param name="context">The bot DB context.</param>
            public Handler(BotContext context)
            {
                _context = context;
            }
            
            /// <inheritdoc />
            public async Task<Result<Tag>> Handle(Command request, CancellationToken cancellationToken)
            {
                var tag = new Tag(
                    request.GuildId.Value,
                    request.TagName,
                    request.Content,
                    DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    request.Author.Value,
                    request.Description);

                try
                {
                    await _context.Tags.AddAsync(tag, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    return e;
                }

                return tag;
            }
        }
    }
}