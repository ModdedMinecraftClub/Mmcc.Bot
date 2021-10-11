using System;
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

namespace Mmcc.Bot.Commands.Moderation.MemberApplications;

/// <summary>
/// Gets the next pending application in the queue.
/// </summary>
public class GetNextPending
{
    /// <summary>
    /// Query to get the next pending application.
    /// </summary>
    public class Query : IRequest<Result<MemberApplication?>>
    {
        /// <summary>
        /// ID of the Guild.
        /// </summary>
        public Snowflake GuildId { get; set; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.GuildId)
                .NotNull();
        }
    }
        
    /// <inheritdoc />
    public class Handler : IRequestHandler<Query, Result<MemberApplication?>>
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
            try
            {
                var res = await _context.MemberApplications
                    .AsNoTracking()
                    .Where(
                        app =>
                            app.AppStatus == ApplicationStatus.Pending
                            && app.GuildId == request.GuildId.Value
                    )
                    .OrderBy(app => app.MemberApplicationId)
                    .FirstOrDefaultAsync(cancellationToken);
                return res;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}