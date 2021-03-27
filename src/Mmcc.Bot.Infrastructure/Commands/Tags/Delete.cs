using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.Tags
{
    /// <summary>
    /// Deletes a tag.
    /// </summary>
    public class Delete
    {
        /// <summary>
        /// Command to delete a tag.
        /// </summary>
        public record Command(Snowflake GuildId, string TagName) : IRequest<Result<Tag>>;
        
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
                try
                {
                    var tag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.GuildId == request.GuildId.Value && t.TagName == request.TagName,
                            cancellationToken);

                    _context.Remove(tag);
                    await _context.SaveChangesAsync(cancellationToken);
                    return tag;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}