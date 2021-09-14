using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Commands.Guilds
{
    /// <summary>
    /// Gets guild info.
    /// </summary>
    public class GetGuildInfo
    {
        /// <summary>
        /// Query to get guild info.
        /// </summary>
        public record Query(Snowflake GuildId) : IRequest<Result<QueryResult>>;

        /// <summary>
        /// Validates the <see cref="Query"/>.
        /// </summary>
        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(q => q.GuildId)
                    .NotNull();
            }
        }

        /// <summary>
        /// Result of the query to get guild info.
        /// </summary>
        public record QueryResult(
            string GuildName,
            Snowflake GuildOwnerId,
            int? GuildMaxMembers,
            IList<IRole> GuildRoles,
            Uri? GuildIconUrl
        );

        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<QueryResult>>
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
            public async Task<Result<QueryResult>> Handle(Query request, CancellationToken cancellationToken)
            {
                var getGuildInfoResult = await _guildApi.GetGuildAsync(request.GuildId, ct: cancellationToken);
                if (!getGuildInfoResult.IsSuccess)
                {
                    return Result<QueryResult>.FromError(getGuildInfoResult);
                }

                var guildInfo = getGuildInfoResult.Entity;
                if (guildInfo is null)
                {
                    return new NotFoundError("Guild not found.");
                }

                Uri? iconUrl;
                if (guildInfo.Icon is not null)
                {
                    var getIconUrlResult = CDN.GetGuildIconUrl(request.GuildId, guildInfo.Icon, CDNImageFormat.PNG);
                    iconUrl = !getIconUrlResult.IsSuccess ? null : getIconUrlResult.Entity;
                }
                else
                {
                    iconUrl = null;
                }

                return new QueryResult(
                    guildInfo.Name,
                    guildInfo.OwnerID,
                    guildInfo.MaxMembers.HasValue ? guildInfo.MaxMembers.Value : null,
                    guildInfo.Roles.Skip(1).ToList(),
                    iconUrl
                );
            }
        }
    }
}