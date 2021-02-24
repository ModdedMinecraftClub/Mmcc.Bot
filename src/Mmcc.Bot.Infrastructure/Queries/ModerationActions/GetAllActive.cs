using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.ModerationActions
{
    /// <summary>
    /// Gets all active moderation actions.
    /// </summary>
    public class GetAllActive
    {
        /// <summary>
        /// Query to get all active moderation actions.
        /// </summary>
        public class Query : IRequest<Result<IList<ModerationAction>>>
        {
            /// <summary>
            /// Whether to enable tracking.
            /// </summary>
            public bool Tracking { get; set; } = true;
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
                    var cmd = _context.ModerationActions
                        .Where(ma => ma.IsActive);

                    if (!request.Tracking)
                    {
                        cmd = cmd.AsNoTracking();
                    }

                    return await cmd.ToListAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}