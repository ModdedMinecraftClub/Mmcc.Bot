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

namespace Mmcc.Bot.Commands.Moderation;

/// <summary>
/// Gets moderation action by ID.
/// </summary>
public class GetById
{
    public record Query(
        int ModerationActionId,
        Snowflake GuildId,
        bool EnableTracking = true
    ) : IRequest<Result<ModerationAction?>>;

    /// <summary>
    /// Validates the <see cref="Query"/>.
    /// </summary>
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.ModerationActionId)
                .NotNull()
                .GreaterThan(0);

            RuleFor(q => q.GuildId)
                .NotNull();
        }
    }
        
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