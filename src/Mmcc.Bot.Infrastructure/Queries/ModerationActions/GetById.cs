using System;
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
    /// Gets moderation action by ID.
    /// </summary>
    public class GetById
    {
        public record Query(int ModerationActionId, Snowflake GuildId, bool EnableTracking = true) : IRequest<Result<ModerationAction?>>;
        
        public class Handler : IRequestHandler<Query, Result<ModerationAction?>>
        {
            private readonly BotContext _context;
            
            public Handler(BotContext context)
            {
                _context = context;
            }

            public async Task<Result<ModerationAction?>> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    var res = await _context.ModerationActions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ma => ma.ModerationActionId == request.ModerationActionId
                                                   && ma.GuildId == request.GuildId.Value, cancellationToken);
                    return res;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}