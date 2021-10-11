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

namespace Mmcc.Bot.Commands.Tags.Usage;

/// <summary>
/// Gets one tag belonging to a guild.
/// </summary>
public class GetOne
{
    /// <summary>
    /// Query to get one tag belonging to a guild by name.
    /// </summary>
    /// <param name="GuildId">The guild ID.</param>
    /// <param name="TagName">The tag name.</param>
    public record Query(Snowflake GuildId, string TagName) : IRequest<Result<Tag?>>;

    /// <summary>
    /// Validates the <see cref="Query"/>.
    /// </summary>
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.GuildId)
                .NotNull();

            RuleFor(q => q.TagName)
                .NotEmpty();
        }
    }
        
    /// <inheritdoc />
    public class Handler : IRequestHandler<Query, Result<Tag?>>
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

        /// <inheritdoc />
        public async Task<Result<Tag?>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                return await _context.Tags
                    .FirstOrDefaultAsync(
                        t => t.GuildId == request.GuildId.Value && t.TagName.Equals(request.TagName),
                        cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}