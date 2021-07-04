using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Abstractions
{
    /// <summary>
    /// Sends DMs.
    /// </summary>
    public interface IDmSender
    {
        /// <summary>
        /// Sends a DM message with <paramref name="message"/> <see cref="string"/> content.
        /// </summary>
        /// <param name="userId">ID of the user to DM.</param>
        /// <param name="message">Message content.</param>
        /// <returns>Result of the asynchronous operation.</returns>
        Task<IResult> Send(Snowflake userId, string message);

        /// <summary>
        /// Sends a DM message with <paramref name="embeds"/> content.
        /// </summary>
        /// <param name="userId">ID of the user to DM.</param>
        /// <param name="embeds">Message content.</param>
        /// <returns>Result of the asynchronous operation.</returns>
        Task<IResult> Send(Snowflake userId, params Embed[] embeds);

        /// <summary>
        /// Sends a DM message with <paramref name="embeds"/> content.
        /// </summary>
        /// <param name="userId">ID of the user to DM.</param>
        /// <param name="embeds">Message content.</param>
        /// <returns>Result of the asynchronous operation.</returns>
        Task<IResult> Send(Snowflake userId, List<Embed> embeds);
    }
    
    /// <inheritdoc />
    public class DmSender : IDmSender
    {
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IDiscordRestChannelAPI _channelApi;

        /// <summary>
        /// Instantiates a new instance of <see cref="DmSender"/>.
        /// </summary>
        /// <param name="userApi">The user API.</param>
        /// <param name="channelApi">The channel API.</param>
        public DmSender(IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi)
        {
            _userApi = userApi;
            _channelApi = channelApi;
        }

        /// <inheritdoc />
        public async Task<IResult> Send(Snowflake userId, string message) =>
            await CreateDm(userId) switch
            {
                { IsSuccess: true, Entity: { } dmChannel } =>
                    await _channelApi.CreateMessageAsync(dmChannel.ID, message),

                { IsSuccess: false } res => res
            };

        /// <inheritdoc />
        public async Task<IResult> Send(Snowflake userId, params Embed[] embeds) =>
            await Send(userId, embeds.ToList());

        /// <inheritdoc />
        public async Task<IResult> Send(Snowflake userId, List<Embed> embeds) =>
            await CreateDm(userId) switch
            {
                { IsSuccess: true, Entity: { } dmChannel } =>
                    await _channelApi.CreateMessageAsync(dmChannel.ID, embeds: embeds),

                { IsSuccess: false } res => res
            };
        
        private async Task<Result<IChannel>> CreateDm(Snowflake userId) =>
            await _userApi.CreateDMAsync(userId);
    }
}