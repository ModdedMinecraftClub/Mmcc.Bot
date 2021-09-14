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

namespace Mmcc.Bot.Commands.Tags.Management
{
    /// <summary>
    /// Updates a tag.
    /// </summary>
    public class Update
    {
        /// <summary>
        /// Command to update a tag.
        /// </summary>
        public record Command(
            Snowflake GuildId,
            Snowflake UpdateAuthor,
            string TagName,
            string? NewDescription,
            string NewContent
        ) : IRequest<Result<Tag>>;
        
        /// <summary>
        /// Validates the <see cref="Command"/>.
        /// </summary>
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.GuildId)
                    .NotNull();

                RuleFor(c => c.UpdateAuthor)
                    .NotNull();

                RuleFor(c => c.TagName)
                    .NotEmpty();

                RuleFor(c => c.NewDescription)
                    .MinimumLength(6).When(c => c.NewDescription is not null);

                RuleFor(c => c.NewContent)
                    .NotEmpty();
            }
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Command, Result<Tag>>
        {
            private readonly BotContext _context;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/>.
            /// </summary>
            /// <param name="context">The bot DB context.</param>
            public Handler(BotContext context)
            {
                _context = context;
            }

            /// <inheritdoc />
            public async Task<Result<Tag>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var tag = await _context.Tags
                        .FirstOrDefaultAsync(t =>
                            t.GuildId == request.GuildId.Value && t.TagName.Equals(request.TagName), cancellationToken);

                    if (tag is null)
                    {
                        return Result<Tag>.FromError(
                            new NotFoundError(
                                $"Could not find tag {request.TagName} for guild {request.GuildId.ToString()}."));
                    }

                    tag.TagDescription = request.NewDescription;
                    tag.Content = request.NewContent;
                    tag.LastModifiedByDiscordId = request.UpdateAuthor.Value;
                    tag.LastModifiedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    await _context.SaveChangesAsync(cancellationToken);
                    return tag;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}