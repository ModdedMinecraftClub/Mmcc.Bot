using System;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Common.Extensions.Remora.Discord.API.Abstractions.Rest;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.Events.Users
{
    /// <summary>
    /// Responds to a Discord user leaving a guild.
    /// </summary>
    public class UserLeftResponder : IResponder<IGuildMemberRemove>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly DiscordSettings _discordSettings;
        private readonly IColourPalette _colourPalette;
        private readonly IDiscordRestGuildAPI _guildApi;

        /// <summary>
        /// Instantiates a new instance of <see cref="UserLeftResponder"/> class.
        /// </summary>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="discordSettings">The Discord settings.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="guildApi">The guild API.</param>
        public UserLeftResponder(
            IDiscordRestChannelAPI channelApi,
            DiscordSettings discordSettings,
            IColourPalette colourPalette,
            IDiscordRestGuildAPI guildApi
        )
        {
            _channelApi = channelApi;
            _discordSettings = discordSettings;
            _colourPalette = colourPalette;
            _guildApi = guildApi;
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync(IGuildMemberRemove ev, CancellationToken ct = default)
        {
            var getLogsChannelResult =
                await _guildApi.FindGuildChannelByName(ev.GuildID, _discordSettings.ChannelNames.LogsSpam);
            if (!getLogsChannelResult.IsSuccess)
            {
                return Result.FromError(getLogsChannelResult.Error);
            }
            
            if (ev.User.IsBot.HasValue && ev.User.IsBot.Value
                || ev.User.IsSystem.HasValue && ev.User.IsSystem.Value
            )
            {
                return Result.FromSuccess();
            }

            var user = ev.User;
            var iconUrl = new Optional<string>();
            var embedThumbnail = new Optional<IEmbedThumbnail>();
            
            if (user.Avatar?.Value is not null)
            {
                var url = $"https://cdn.discordapp.com/avatars/{user.ID.Value}/{user.Avatar.Value}.png";
                iconUrl = url;
                embedThumbnail = new EmbedThumbnail(url, new(), new(), new());
            }
            
            var embed = new Embed
            {
                Author = new EmbedAuthor($"{user.Username}#{user.Discriminator}", $"https://discord.com/users/{user.ID}",  iconUrl, new()),
                Description = $":outbox_tray: <@{user.ID}> left the server.",
                Thumbnail = embedThumbnail,
                Colour = _colourPalette.Red,
                Footer = new EmbedFooter($"ID: {user.ID}", new(), new()),
                Timestamp = DateTimeOffset.UtcNow
            };
            var sendMessageResult =
                await _channelApi.CreateMessageAsync(getLogsChannelResult.Entity.ID, embeds: new[] { embed }, ct: ct);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
    }
}