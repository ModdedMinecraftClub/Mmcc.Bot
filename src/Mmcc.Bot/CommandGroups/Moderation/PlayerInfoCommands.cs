using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Extensions.Database.Entities;
using Mmcc.Bot.Core.Extensions.Models;
using Mmcc.Bot.Core.Extensions.Remora.Discord.API.Abstractions.Objects;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries.ModerationActions;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Moderation
{
    /// <summary>
    /// Commands for obtaining information about players.
    /// </summary>
    [Group("info")]
    [Description("Information about players")]
    [RequireGuild]
    public class PlayerInfoCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;
        private readonly IMojangApiService _mojangApi;
        private readonly IDiscordRestGuildAPI _guildApi;

        /// <summary>
        /// Instantiates a new instance of <see cref="PlayerInfoCommands"/>.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="mojangApi">The Mojang API.</param>
        /// <param name="guildApi">The guild API.</param>
        public PlayerInfoCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            IMediator mediator,
            ColourPalette colourPalette,
            IMojangApiService mojangApi,
            IDiscordRestGuildAPI guildApi
        )
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _colourPalette = colourPalette;
            _mojangApi = mojangApi;
            _guildApi = guildApi;
        }
        
        /// <summary>
        /// Views info about a Discord user..
        /// </summary>
        /// <param name="user">Discord user.</param>
        /// <returns>Result of the operation.</returns>
        [Command("discord", "d")]
        [Description("Obtains information about a Discord user")]
        public async Task<IResult> InfoDiscord(IUser user)
        {
            var fields = new List<EmbedField>();
            var iconUrl = new Optional<string>();
            var embedThumbnail = new Optional<IEmbedThumbnail>();

            if (user.Avatar?.Value is not null)
            {
                var url = CDN.GetUserAvatarUrl(user, CDNImageFormat.PNG);
                
                if (url.IsSuccess)
                {
                    var urlString = url.Entity.ToString();
                    iconUrl = urlString;
                    embedThumbnail = new EmbedThumbnail(urlString);
                }
            }
            
            var embed = new Embed
            {
                Author = new EmbedAuthor($"{user.Username}#{user.Discriminator}", $"https://discord.com/users/{user.ID}",  iconUrl, new()),
                Thumbnail = embedThumbnail
            };
            
            fields.Add(user.GetEmbedField());
            
            var getGuildMemberResult = await _guildApi.GetGuildMemberAsync(_context.Message.GuildID.Value, user.ID);
            
            if (getGuildMemberResult.IsSuccess)
            {
                var guildMember = getGuildMemberResult.Entity;
                var guildParticipationEmbedFieldValue = new StringBuilder();
                guildParticipationEmbedFieldValue.AppendLine($"Joined at: {guildMember.JoinedAt.UtcDateTime} UTC");

                var rolesStrB = new StringBuilder();
                foreach (var roleId in guildMember.Roles)
                {
                    rolesStrB.Append($"<@&{roleId.Value}>");
                }

                guildParticipationEmbedFieldValue.AppendLine($"Roles: {rolesStrB}");
                fields.Add(new EmbedField(":people_hugging: Guild participation",
                    guildParticipationEmbedFieldValue.ToString(), false));
            }
            else
            {
                fields.Add(new EmbedField(":people_hugging: Guild participation",
                    "The user is not a member of the current guild.", false));
            }

            var queryResult =
                await _mediator.Send(new GetByDiscordId.Query(_context.Message.GuildID.Value, user.ID.Value));
            
            if (queryResult.IsSuccess)
            {
                var moderationFields =
                    queryResult.Entity.GetEmbedFields(showAssociatedDiscord: false, showAssociatedIgn: true);
                fields.AddRange(moderationFields);
            }
            else
            {
                fields.Add(new EmbedField(":regional_indicator_m: Moderation events",
                    $":x: Error obtaining moderation events: {queryResult.Error.Message}", false));
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

        /// <summary>
        /// Views info about a player by IGN.
        /// </summary>
        /// <param name="ign">IGN.</param>
        /// <returns>The result of the operation.</returns>
        [Command("ig")]
        [Description("Obtains information about a player by IGN")]
        public async Task<IResult> InfoIg(string ign)
        {
            var embed = new Embed
            {
                Title = ign,
                Colour = _colourPalette.Blue,
                Thumbnail = EmbedProperties.MmccLogoThumbnail
            };
            var fields = new List<EmbedField>();
            var queryResult = await _mediator.Send(new GetByIgn.Query(_context.Message.GuildID.Value, ign));
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
                    fields.Add(getNamesHistory.Entity.GetEmbedField());
                }
            }

            if (queryResult.IsSuccess)
            {
                var moderationFields =
                    queryResult.Entity.GetEmbedFields(showAssociatedDiscord: true, showAssociatedIgn: false);
                fields.AddRange(moderationFields);
            }
            else
            {
                fields.Add(new EmbedField(":regional_indicator_m: Moderation events",
                    $":x: Error: {queryResult.Error.Message}", false));
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