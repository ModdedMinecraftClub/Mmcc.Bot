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

namespace Mmcc.Bot.Commands.Tags.Management;

/// <summary>
/// Gets all tags belonging to a guild.
/// </summary>
public class GetAll
{
    /// <summary>
    /// Query to get all tags belonging to a guild.
    /// </summary>
    /// <param name="GuildId">The guild ID.</param>
    public record Query(Snowflake GuildId) : IRequest<Result<IList<Tag>>>;

    /// <summary>
    /// Validates the <see cref="Query"/>.
    /// </summary>
    public class Validator : AbstractValidator<Query>
    {
        public Validator() =>
            RuleFor(q => q.GuildId)
                .NotNull();
    }

    /// <inheritdoc />
    public class Handler : IRequestHandler<Query, Result<IList<Tag>>>
    {
        private readonly BotContext _context;

        /// <summary>
        /// Instantiates a new instance of the <see cref="Handler"/> class.
        /// </summary>
        /// <param name="context">The bot DB context.</param>
        public Handler(BotContext context)
        {
            _context = context;
        }

        public async Task<Result<IList<Tag>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.Tags
                    .Where(t => t.GuildId == request.GuildId.Value)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}