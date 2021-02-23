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
    /// Gets a Guild channel by name.
    /// </summary>
    public class GetChannelByName
    {
        /// <summary>
        /// Query to get the member apps channel.
        /// </summary>
        public class Query : IRequest<Result<IChannel>>
        {
            /// <summary>
            /// Guild ID.
            /// </summary>
            public Snowflake GuildId { get; set; }
            
            /// <summary>
            /// Channel name.
            /// </summary>
            public string ChannelName { get; set; } = null!;
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Query, Result<IChannel>>
        {
            private readonly IDiscordRestGuildAPI _guildApi;

            public Handler(IDiscordRestGuildAPI guildApi, DiscordSettings settings)
            {
                _guildApi = guildApi;
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
                
                var channel = guildChannels
                    .Where(c => c.Name.HasValue && c.Name.Value is not null)
                    .FirstOrDefault(c => c.Name.Value!.Equals(request.ChannelName));
                if (channel is null)
                {
                    return new NotFoundError(
                        $"Could not find the channel with name {request.ChannelName} in guild {request.GuildId}.");
                }

                return Result<IChannel>.FromSuccess(channel);
            }
        }
    }
}