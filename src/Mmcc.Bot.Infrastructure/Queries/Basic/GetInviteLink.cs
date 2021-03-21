using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using MoreLinq;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.Basic
{
    /// <summary>
    /// Gets an invite link to a guild.
    /// </summary>
    public class GetInviteLink
    {
        /// <summary>
        /// Query to get an invite link to a guild.
        /// </summary>
        public record Query(Snowflake GuildId) : IRequest<Result<string>>;

        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<string>>
        {
            private readonly IDiscordRestGuildAPI _guildApi;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/> class.
            /// </summary>
            /// <param name="guildApi">The guild API.</param>
            public Handler(IDiscordRestGuildAPI guildApi)
            {
                _guildApi = guildApi;
            }

            /// <inheritdoc />
            public async Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
            {
                var getGuildInvitesResult = await _guildApi.GetGuildInvitesAsync(request.GuildId, cancellationToken);

                if (!getGuildInvitesResult.IsSuccess)
                {
                    return Result<string>.FromError(getGuildInvitesResult);
                }

                var invites = getGuildInvitesResult.Entity;

                if (invites is null || !invites.Any())
                {
                    return new NotFoundError(
                        "Could not find an active invite link. Administrators should create an invite link in Discord guild settings that does not expire.");
                }

                var inv = invites[0].Code;
                return Result<string>.FromSuccess(inv);
            }
        }
    }
}