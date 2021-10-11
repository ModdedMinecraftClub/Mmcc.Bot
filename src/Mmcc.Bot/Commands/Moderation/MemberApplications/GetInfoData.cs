using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Mmcc.Bot.Common.Models.Settings;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Commands.Moderation.MemberApplications;

/// <summary>
/// Gets data necessary for the member application info embed.
/// </summary>
public class GetInfoData
{
    /// <summary>
    /// Query to get data necessary for the member application info embed.
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
    /// Result payload of <see cref="Query"/>.
    /// </summary>
    public record QueryResult(Snowflake MemberAppsChannelId, Snowflake StaffRoleId);
        
    public class Handler : IRequestHandler<Query, Result<QueryResult>>
    {
        private readonly IDiscordRestGuildAPI _guildApi;
            
        private readonly ChannelNamesSettings _channelNames;
        private readonly RoleNamesSettings _roleNames;

        public Handler(IDiscordRestGuildAPI guildApi, DiscordSettings discordSettings)
        {
            _guildApi = guildApi;
            _channelNames = discordSettings.ChannelNames;
            _roleNames = discordSettings.RoleNames;
        }

        public async Task<Result<QueryResult>> Handle(Query request, CancellationToken cancellationToken)
        {
            var getGuildRolesResult = await _guildApi.GetGuildRolesAsync(request.GuildId, cancellationToken);
            if (!getGuildRolesResult.IsSuccess)
            {
                return Result<QueryResult>.FromError(getGuildRolesResult);
            }

            var getGuildChannelsResult = await _guildApi.GetGuildChannelsAsync(request.GuildId, cancellationToken);
            if (!getGuildChannelsResult.IsSuccess)
            {
                return Result<QueryResult>.FromError(getGuildChannelsResult);
            }
                
            var roles = getGuildRolesResult.Entity;
            if (roles is null)
            {
                return new NotFoundError($"Could not find roles in guild {request.GuildId}.");
            }

            var channels = getGuildChannelsResult.Entity;
            if (channels is null)
            {
                return new NotFoundError($"Could not find channels in guild {request.GuildId}.");
            }

            var staffRole = roles.FirstOrDefault(r => r.Name.Equals(_roleNames.Staff));
            if (staffRole is null)
            {
                return new NotFoundError(
                    $"Could not find Staff role ({_roleNames.Staff}) in guild {request.GuildId}.");
            }

            var membersChannel = channels.FirstOrDefault(c => c.Name.Equals(_channelNames.MemberApps));
            if (membersChannel is null)
            {
                return new NotFoundError(
                    $"Could not find member apps channel (#{_channelNames.MemberApps}) in guild {request.GuildId}.");
            }

            return new QueryResult(membersChannel.ID, staffRole.ID);
        }
    }
}