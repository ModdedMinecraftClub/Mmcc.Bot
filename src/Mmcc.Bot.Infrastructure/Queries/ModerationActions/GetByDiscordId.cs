using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.ModerationActions
{
    /// <summary>
    /// Gets moderation actions for a given Discord user.
    /// </summary>
    public class GetByDiscordId
    {
        /// <summary>
        /// Query to get moderation actions by Discord user ID.
        /// </summary>
        public record Query(Snowflake GuildId, ulong DiscordUserId) : IRequest<Result<IList<ModerationAction>>>;

        /// <summary>
        /// Validates the <see cref="Query"/>.
        /// </summary>
        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(q => q.GuildId)
                    .NotNull();

                RuleFor(q => q.DiscordUserId)
                    .NotNull();
            }
        }

        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<IList<ModerationAction>>>
        {
            private readonly BotContext _context;
            
            /// <summary>
            /// Instantiates a new instance of <see cref="BotContext"/> class.
            /// </summary>
            /// <param name="context">The db context.</param>
            public Handler(BotContext context)
            {
                _context = context;
            }
            
            /// <inheritdoc />
            public async Task<Result<IList<ModerationAction>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var res = _context.ModerationActions
                        .AsNoTracking()
                        .Where(ma =>
                            ma.UserDiscordId != null && ma.UserDiscordId == request.DiscordUserId &&
                            ma.GuildId == request.GuildId.Value);
                    return await res.ToListAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}