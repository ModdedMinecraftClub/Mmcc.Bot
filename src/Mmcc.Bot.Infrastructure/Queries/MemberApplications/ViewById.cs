using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.MemberApplications
{
    /// <summary>
    /// Views a member application by ID, provided it belongs to the given guild.
    /// </summary>
    public class ViewById
    {
        /// <summary>
        /// Query to get a member application by ID.
        /// </summary>
        public class Query : IRequest<Result<MemberApplication?>>
        {
            /// <summary>
            /// ID of the application
            /// </summary>
            public int ApplicationId { get; set; }
        }
        
        private class Handler : IRequestHandler<Query, Result<MemberApplication?>>
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
            public async Task<Result<MemberApplication?>> Handle(Query request, CancellationToken cancellationToken)
            {
                var res = await _context.MemberApplications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(app => app.MemberApplicationId == request.ApplicationId, cancellationToken);
                return res;
            }
        }
    }
}