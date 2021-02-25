using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Infrastructure.Queries.Discord;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.Responders.Users
{
    /// <summary>
    /// Responds to a Discord user joining a guild.
    /// </summary>
    public class UserJoinedResponder : IResponder<IGuildMemberAdd>
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly DiscordSettings _discordSettings;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;

        /// <summary>
        /// Instantiates a new instance of <see cref="UserJoinedResponder"/> class.
        /// </summary>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="discordSettings">The Discord settings.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public UserJoinedResponder(
            IDiscordRestChannelAPI channelApi,
            DiscordSettings discordSettings,
            IMediator mediator,
            ColourPalette colourPalette
        )
        {
            _channelApi = channelApi;
            _discordSettings = discordSettings;
            _mediator = mediator;
            _colourPalette = colourPalette;
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync(IGuildMemberAdd ev, CancellationToken ct = default)
        {
            var getLogsChannelResult = await _mediator.Send(new GetChannelByName.Query
                {GuildId = ev.GuildID, ChannelName = _discordSettings.ChannelNames.LogsSpam}, ct);
            if (!getLogsChannelResult.IsSuccess)
            {
                return Result.FromError(getLogsChannelResult.Error);
            }

            if (
                !ev.User.HasValue
                || ev.User.Value is null
                || ev.User.Value.IsBot.HasValue && ev.User.Value.IsBot.Value
                || ev.User.Value.IsSystem.HasValue && ev.User.Value.IsSystem.Value
            )
            {
                return Result.FromSuccess();
            }

            var user = ev.User.Value;
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
                Description = $":inbox_tray: <@{user.ID}> joined the server.",
                Thumbnail = embedThumbnail,
                Colour = _colourPalette.Green,
                Footer = new EmbedFooter($"ID: {user.ID}", new(), new()),
                Timestamp = DateTimeOffset.UtcNow
            };
            var sendMessageResult =
                await _channelApi.CreateMessageAsync(getLogsChannelResult.Entity.ID, embed: embed, ct: ct);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
    }
}