using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Infrastructure.Commands.Tags;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Tags
{
    /// <summary>
    /// Tags commands.
    /// </summary>
    [Group("tags")]
    [RequireGuild]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public class TagsManagementCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;

        /// <summary>
        /// Instantiates a new instance of <see cref="TagsManagementCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public TagsManagementCommands(MessageContext context, IDiscordRestChannelAPI channelApi, IMediator mediator, ColourPalette colourPalette)
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _colourPalette = colourPalette;
        }

        [Command("create")]
        [Description("Creates a new tag for the current guild.")]
        public async Task<IResult> CreateNewTag(string tagName, string? description, [Greedy] string content)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                description = null;
            }

            var commandResult = await _mediator.Send(new Create.Command(_context.GuildID.Value, _context.User.ID,
                tagName, description, content));

            if (!commandResult.IsSuccess)
            {
                return commandResult;
            }

            var tag = commandResult.Entity;
            var embed = new Embed
            {
                Title = "Tag created.",
                Description = "The tag has been successfully created.",
                Fields = new List<EmbedField>
                {
                    new("Tag name", tag.Content, false),
                    new("Tag description", tag.TagDescription ?? "None", false),
                    new("Created by", $"<@{tag.CreatedByDiscordId}>")
                },
                Colour = _colourPalette.Green,
                Timestamp = DateTimeOffset.UtcNow
            };
            return await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
        }

        [Command("update")]
        [Description("Updates a tag belonging to the current guild.")]
        public async Task<IResult> UpdateTag(string tagName, string? description, [Greedy] string content)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                description = null;
            }

            var commandResult = await _mediator.Send(new Update.Command(_context.GuildID.Value, _context.User.ID,
                tagName, description, content));
            
            if (!commandResult.IsSuccess)
            {
                return commandResult;
            }
            
            var tag = commandResult.Entity;
            var embed = new Embed
            {
                Title = "Tag updated.",
                Description = "The tag has been successfully updated.",
                Fields = new List<EmbedField>
                {
                    new("Tag name", tag.Content, false),
                    new("Tag description", tag.TagDescription ?? "None", false),
                    new("Updated by", $"<@{tag.LastModifiedByDiscordId}>")
                },
                Colour = _colourPalette.Green,
                Timestamp = DateTimeOffset.UtcNow
            };
            return await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
        }

        [Command("delete", "del")]
        [Description("Deletes a tag belonging to the current guild.")]
        public async Task<IResult> DeleteTag(string tagName)
        {
            var commandResult = await _mediator.Send(new Delete.Command(_context.GuildID.Value, tagName));
            
            if (!commandResult.IsSuccess)
            {
                return commandResult;
            }
            
            var embed = new Embed
            {
                Title = "Tag deleted.",
                Description = "The tag has been successfully deleted.",
                Fields = new List<EmbedField>
                {
                    new("Tag name", tagName, false)
                },
                Colour = _colourPalette.Green,
                Timestamp = DateTimeOffset.UtcNow
            };
            return await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
        }
    }
}