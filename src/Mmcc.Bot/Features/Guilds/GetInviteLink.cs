using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.Features.Guilds;

/// <summary>
/// Gets an invite link to a guild.
/// </summary>
public class GetInviteLink
{
    /// <summary>
    /// Query to get an invite link to a guild.
    /// </summary>
    public record Query(Snowflake GuildId) : IRequest<Result<string>>;

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
    
    public class Handler : IRequestHandler<Query, Result<string>>
    {
        private readonly IDiscordRestGuildAPI _guildApi;
        
        public Handler(IDiscordRestGuildAPI guildApi)
        {
            _guildApi = guildApi;
        }
        
        public async Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var getGuildInvitesResult = await _guildApi.GetGuildInvitesAsync(request.GuildId, cancellationToken);
            if (!getGuildInvitesResult.IsSuccess)
            {
                return Result<string>.FromError(getGuildInvitesResult);
            }

            var invites = getGuildInvitesResult.Entity;
            if (!invites.Any())
            {
                return new NotFoundError(
                    "Could not find an active invite link. Administrators should create an invite link in Discord guild settings that does not expire.");
            }

            var inv = invites[0].Code;
            return Result<string>.FromSuccess(inv);
        }
    }
}