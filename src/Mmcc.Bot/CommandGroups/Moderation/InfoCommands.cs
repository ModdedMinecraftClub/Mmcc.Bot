using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Extensions.Database.Entities;
using Mmcc.Bot.Core.Extensions.Models;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries.ModerationActions;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Moderation
{
    /// <summary>
    /// Commands for obtaining information about players.
    /// </summary>
    [Group("info")]
    [Description("Information about players")]
    public class InfoCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;
        private readonly IMojangApiService _mojangApi;

        /// <summary>
        /// Instantiates a new instance of <see cref="InfoCommands"/>.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="mojangApi">The Mojang API.</param>
        public InfoCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            IMediator mediator,
            ColourPalette colourPalette,
            IMojangApiService mojangApi
        )
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _colourPalette = colourPalette;
            _mojangApi = mojangApi;
        }

        /// <summary>
        /// Views info about a player by IGN.
        /// </summary>
        /// <param name="ign">IGN.</param>
        /// <returns>The result of the operation.</returns>
        [Command("ig")]
        [Description("Obtains information about a player by IGN")]
        [RequireGuild]
        public async Task<IResult> InfoIg(string ign)
        {
            var embed = new Embed
            {
                Title = ign,
                Colour = _colourPalette.Blue,
                Thumbnail = EmbedProperties.MmccLogoThumbnail
            };
            var fields = new List<EmbedField>();
            var queryResult = await _mediator.Send(
                new GetByIgn.Query
                {
                    Ign = ign,
                    GuildId = _context.Message.GuildID.Value
                }
            );
            var getUuid = await _mojangApi.GetPlayerUuidInfo(ign);
            
            if (getUuid.IsSuccess && getUuid.Entity is not null)
            {
                embed = embed with
                {
                    Description = $"Minecraft UUID: `{getUuid.Entity.Id}`"
                };

                var getNamesHistory = await _mojangApi.GetNameHistory(getUuid.Entity.Id);
                if (getNamesHistory.IsSuccess && getNamesHistory.Entity is not null)
                {
                    fields.Add(getNamesHistory.Entity.ToEmbedField());
                }
            }

            if (queryResult.IsSuccess)
            {
                var moderationFields =
                    queryResult.Entity.ToEmbedFields(showAssociatedDiscord: true, showAssociatedIgn: false);
                fields.AddRange(moderationFields);
            }

            embed = embed with
            {
                Fields = fields,
                Timestamp = DateTimeOffset.Now
            };
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
    }
}