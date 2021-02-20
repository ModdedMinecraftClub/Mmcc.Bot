using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Models.Settings;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries.Discord
{
    /// <summary>
    /// Gets the member apps channel.
    /// </summary>
    public class GetMembersChannel
    {
        /// <summary>
        /// Query to get the member apps channel.
        /// </summary>
        public class Query : IRequest<Result<IChannel>>
        {
            public Snowflake GuildId { get; set; }
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<IChannel>>
        {
            private readonly IDiscordRestGuildAPI _guildApi;
            private readonly DiscordSettings _settings;

            public Handler(IDiscordRestGuildAPI guildApi, DiscordSettings settings)
            {
                _guildApi = guildApi;
                _settings = settings;
            }
            
            /// <inheritdoc />
            public async Task<Result<IChannel>> Handle(Query request, CancellationToken cancellationToken)
            {
                var getGuildChannelsResult = await _guildApi.GetGuildChannelsAsync(request.GuildId, cancellationToken);
                if (!getGuildChannelsResult.IsSuccess)
                {
                    return Result<IChannel>.FromError(getGuildChannelsResult);
                }
                
                var guildChannels = getGuildChannelsResult.Entity;
                if (guildChannels is null)
                {
                    return new NotFoundError("Guild channels for current guild not found.");
                }

                var memberAppsChannelName = _settings.ChannelNames.MemberApps;
                var memberAppsChannel = guildChannels
                    .Where(c => c.Name.HasValue && c.Name.Value is not null)
                    .FirstOrDefault(c => c.Name.Value!.Equals(memberAppsChannelName));
                if (memberAppsChannel is null)
                {
                    return new NotFoundError(
                        $"Could not find the channel with member application. The given member applications channel name was {_settings.ChannelNames.MemberApps}");
                }

                return Result<IChannel>.FromSuccess(memberAppsChannel);
            }
        }
    }
}