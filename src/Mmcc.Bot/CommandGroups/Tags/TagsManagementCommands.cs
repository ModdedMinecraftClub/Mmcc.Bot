using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Infrastructure.Abstractions;
using Mmcc.Bot.Infrastructure.Commands.Tags;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries.Tags;
using MoreLinq;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Tags
{
    /// <summary>
    /// Tags commands.
    /// </summary>
    [Group("tags")]
    [Description("Tags management commands")]
    [RequireGuild]
    public class TagsManagementCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;
        private readonly ICommandResponder _responder;

        /// <summary>
        /// Instantiates a new instance of <see cref="TagsManagementCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="responder">The command responder.</param>
        public TagsManagementCommands(
            MessageContext context,
            IMediator mediator,
            ColourPalette colourPalette,
            ICommandResponder responder
        )
        {
            _context = context;
            _mediator = mediator;
            _colourPalette = colourPalette;
            _responder = responder;
        }

        [Command("create")]
        [Description("Creates a new tag for the current guild.")]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> CreateNewTag(string tagName, string? description, [Greedy] string content)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                description = null;
            }

            return await _mediator.Send(new Create.Command(_context.GuildID.Value, _context.User.ID,
                    tagName, description, content)) switch
                {
                    { IsSuccess: true, Entity: { } e } =>
                        await _responder.Respond(new Embed
                        {
                            Title = "Tag created.",
                            Description = "The tag has been successfully created.",
                            Fields = new List<EmbedField>
                            {
                                new("Tag name", e.Content, false),
                                new("Tag description", e.TagDescription ?? "None", false),
                                new("Created by", $"<@{e.CreatedByDiscordId}>")
                            },
                            Colour = _colourPalette.Green,
                            Timestamp = DateTimeOffset.UtcNow
                        }),

                    { IsSuccess: false } res => res
                };
        }

        [Command("update")]
        [Description("Updates a tag belonging to the current guild.")]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> UpdateTag(string tagName, string? description, [Greedy] string content)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                description = null;
            }

            return await _mediator.Send(new Update.Command(_context.GuildID.Value, _context.User.ID,
                    tagName, description, content)) switch
                {
                    { IsSuccess: true, Entity: { } e } =>
                        await _responder.Respond(new Embed
                        {
                            Title = "Tag updated.",
                            Description = "The tag has been successfully updated.",
                            Fields = new List<EmbedField>
                            {
                                new("Tag name", e.Content, false),
                                new("Tag description", e.TagDescription ?? "None", false),
                                new("Updated by", $"<@{e.LastModifiedByDiscordId}>")
                            },
                            Colour = _colourPalette.Green,
                            Timestamp = DateTimeOffset.UtcNow
                        }),

                    { IsSuccess: false } res => res
                };
        }

        [Command("delete", "del")]
        [Description("Deletes a tag belonging to the current guild.")]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> DeleteTag(string tagName) =>
            await _mediator.Send(new Delete.Command(_context.GuildID.Value, tagName)) switch
            {
                { IsSuccess: true, Entity: { } } =>
                    await _responder.Respond(new Embed
                    {
                        Title = "Tag deleted.",
                        Description = "The tag has been successfully deleted.",
                        Fields = new List<EmbedField>
                        {
                            new("Tag name", tagName, false)
                        },
                        Colour = _colourPalette.Green,
                        Timestamp = DateTimeOffset.UtcNow
                    }),

                { IsSuccess: false } res => res
            };

        [Command("list", "l")]
        [Description("Lists all tags belonging to the current guild.")]
        public async Task<IResult> ListTags() =>
            await _mediator.Send(new GetAll.Query(_context.GuildID.Value)) switch
            {
                { IsSuccess: true, Entity: { } e } when e.Any() =>
                    await _responder.Respond(
                        e
                            .Batch(20)
                            .Select(tags => new Embed
                            {
                                Title = "Tags",
                                Fields = tags
                                    .Select(t =>
                                        new EmbedField($"!t {t.TagName}", t.TagDescription ?? "No description")
                                    ).ToList(),
                                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                                Colour = _colourPalette.Blue
                            })
                            .ToList()
                    ),

                { IsSuccess: true, Entity: { } } =>
                    Result.FromError(new NotFoundError("No tags found.")),

                { IsSuccess: false } res => res
            };
    }
}