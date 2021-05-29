using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.MemberApplications
{
    /// <summary>
    /// Gets a member application by ID, provided it belongs to the given guild.
    /// </summary>
    public class GetById
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
            
            /// <summary>
            /// ID of the Guild.
            /// </summary>
            public Snowflake GuildId { get; set; }
        }

        /// <summary>
        /// Validates the <see cref="Query"/>.
        /// </summary>
        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(q => q.ApplicationId)
                    .NotEmpty();

                RuleFor(q => q.GuildId)
                    .NotEmpty();
            }
        }
        
        /// <inheritdoc />
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
                try
                {
                    var res = await _context.MemberApplications
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            app => app.MemberApplicationId == request.ApplicationId &&
                                   app.GuildId == request.GuildId.Value,
                            cancellationToken);
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