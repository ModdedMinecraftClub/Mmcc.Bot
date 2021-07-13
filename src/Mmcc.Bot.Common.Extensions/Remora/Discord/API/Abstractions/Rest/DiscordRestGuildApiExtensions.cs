using System.Linq;
using System.Threading.Tasks;
using Mmcc.Bot.Common.Errors;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Common.Extensions.Remora.Discord.API.Abstractions.Rest
{
    /// <summary>
    /// Extensions for <see cref="IDiscordRestGuildAPI"/>.
    /// </summary>
    public static class DiscordRestGuildApiExtensions
    {
        /// <summary>
        /// Finds a Discord channel in a Discord guild by channel name.
        /// </summary>
        /// <param name="guildApi">The guild API.</param>
        /// <param name="guildId">The ID of the guild in which to look for the channel.</param>
        /// <param name="channelName">The name of the channel to find.</param>
        /// <returns>Result of the operation. If successful contains the matching <see cref="IChannel"/>.</returns>
        /// <remarks>If multiple channels are found returns the first match.</remarks>
        public static async Task<Result<IChannel>> FindGuildChannelByName(this IDiscordRestGuildAPI guildApi,
            Snowflake guildId,
            string channelName)
        {
            var getGuildChannelsResult = await guildApi.GetGuildChannelsAsync(guildId);
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
                .FirstOrDefault(c => c.Name.Value!.Equals(channelName));
            if (channel is null)
            {
                return new NotFoundError(
                    $"Could not find the channel with name {channelName} in guild {guildId}.");
            }

            return Result<IChannel>.FromSuccess(channel);
        }
    }
}