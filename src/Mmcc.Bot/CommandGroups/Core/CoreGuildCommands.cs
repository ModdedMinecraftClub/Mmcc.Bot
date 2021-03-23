using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries.Basic;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Core
{
    /// <summary>
    /// Core commands.
    /// </summary>
    [RequireGuild]
    public class CoreGuildCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ColourPalette _colourPalette;
        private readonly IMediator _mediator;

        /// <summary>
        /// Instantiates a new instance of <see cref="CoreGuildCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="mediator">The mediator.</param>
        public CoreGuildCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            ColourPalette colourPalette,
            IMediator mediator
        )
        {
            _context = context;
            _channelApi = channelApi;
            _colourPalette = colourPalette;
            _mediator = mediator;
        }

        [Command("guild")]
        [Description("Provides information about the current guild.")]
        public async Task<IResult> GuildInfo()
        {
            var queryResult = await _mediator.Send(new GetGuildInfo.Query(_context.GuildID.Value));

            if (!queryResult.IsSuccess)
            {
                return queryResult;
            }

            var guildInfo = queryResult.Entity;
            var embed = new Embed
            {
                Title = "Guild info",
                Description = "Information about the current guild.",
                Fields = new List<EmbedField>
                {
                    new("Name", guildInfo.GuildName, false),
                    new("Owner", $"<@{guildInfo.GuildOwnerId}>"),
                    new("Max members", guildInfo.GuildMaxMembers.ToString() ?? "Unavailable", false),
                    new("Available roles", string.Join(", ", guildInfo.GuildRoles.Select(r => $"<@&{r.ID}>")))
                },
                Timestamp = DateTimeOffset.UtcNow,
                Colour = _colourPalette.Blue
            };

            if (guildInfo.GuildIconUrl is not null)
            {
                embed = embed with {Thumbnail = new EmbedThumbnail(guildInfo.GuildIconUrl.ToString())};
            }

            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }

        [Command("invite")]
        [Description("Gives an invite link to the current guild.")]
        public async Task<IResult> Invite()
        {
            var queryResult = await _mediator.Send(new GetInviteLink.Query(_context.GuildID.Value));

            if (!queryResult.IsSuccess)
            {
                return queryResult;
            }

            var msg = $"https://discord.gg/{queryResult.Entity}";
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, msg);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
    }
}