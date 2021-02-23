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
        public class Query : IRequest<Result<IList<ModerationAction>>>
        {
            /// <summary>
            /// ID of the guild.
            /// </summary>
            public Snowflake GuildId { get; set; }

            /// <summary>
            /// Minecraft IGN of the user.
            /// </summary>
            public string Ign { get; set; } = null!;
        }
        
        public class Handler : IRequestHandler<Query, Result<IList<ModerationAction>>>
        {
            private readonly BotContext _context;

            public Handler(BotContext context)
            {
                _context = context;
            }

            public async Task<Result<IList<ModerationAction>>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var res = _context.ModerationActions
                        .AsNoTracking()
                        .Where(ma => ma.UserIgn != null && ma.UserIgn.Equals(request.Ign));
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