using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries.Basic;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Core
{
    /// <summary>
    /// Core commands.
    /// </summary>
    public class CoreGuildCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly ColourPalette _colourPalette;
        private readonly IMediator _mediator;

        /// <summary>
        /// Instantiates a new instance of <see cref="CoreGuildCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="guildApi">The guild API.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="mediator">The mediator.</param>
        public CoreGuildCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            IDiscordRestGuildAPI guildApi,
            ColourPalette colourPalette,
            IMediator mediator
        )
        {
            _context = context;
            _channelApi = channelApi;
            _guildApi = guildApi;
            _colourPalette = colourPalette;
            _mediator = mediator;
        }

        [Command("guild")]
        [Description("Provides information about the current guild.")]
        [RequireGuild]
        // TODO: separate into a mediator query;
        public async Task<IResult> GuildInfo()
        {
            var getGuildInfoResult = await _guildApi.GetGuildAsync(_context.GuildID.Value);
            if (!getGuildInfoResult.IsSuccess)
            {
                return getGuildInfoResult;
            }

            var guildInfo = getGuildInfoResult.Entity;
            if (guildInfo is null)
            {
                return Result.FromError(new NotFoundError("Guild not found."));
            }

            var embed = new Embed
            {
                Title = "Guild info",
                Description = "Information about the current guild.",
                Fields = new List<EmbedField>
                {
                    new("Name", guildInfo.Name, false),
                    new("Owner", $"<@{guildInfo.OwnerID}>"),
                    new("Max members",
                        guildInfo.MaxMembers.HasValue
                            ? guildInfo.MaxMembers.Value.ToString()
                            : "Unavailable", false),
                    new("Available roles",
                        string.Join(", ",
                            guildInfo.Roles
                                .Skip(1)
                                .Select(r => $"<@&{r.ID}>")))
                },
                Timestamp = DateTimeOffset.UtcNow,
                Colour = _colourPalette.Blue,
            };

            if (guildInfo.Icon is not null)
            {
                var getIconUrlResult = CDN.GetGuildIconUrl(_context.GuildID.Value, guildInfo.Icon, CDNImageFormat.PNG);
                
                if (getIconUrlResult.IsSuccess)
                {
                    embed = embed with
                    {
                        Thumbnail = new EmbedThumbnail(getIconUrlResult.Entity.ToString())
                    };
                }
            }

            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }

        [Command("invite")]
        [Description("Gives an invite link to the current guild.")]
        [RequireGuild]
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