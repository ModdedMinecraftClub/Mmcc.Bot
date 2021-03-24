using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.ModerationActions
{
    /// <summary>
    /// Gets moderation actions for a given IGN.
    /// </summary>
    public class GetByIgn
    {
        /// <summary>
        /// Query to get moderation actions by IGN.
        /// </summary>
        public record Query(Snowflake GuildId, string Ign) : IRequest<Result<IList<ModerationAction>>>;

        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<IList<ModerationAction>>>
        {
            private readonly BotContext _context;
            
            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/> class.
            /// </summary>
            /// <param name="context">DB context.</param>
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
                            ma.UserIgn != null
                            && ma.UserIgn.Equals(request.Ign)
                            && ma.GuildId == request.GuildId.Value);
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