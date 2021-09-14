using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Commands.Moderation.Bans;
using Mmcc.Bot.Common.Extensions.Database.Entities;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Results;
using Remora.Results;

namespace Mmcc.Bot.Commands.Moderation
{
    /// <summary>
    /// General moderation commands that do not fit into any specific categories.
    /// </summary>
    [Group("moderation", "mod")]
    [Description("Moderation (general)")]
    [RequireGuild]
    public class GeneralModerationCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IMediator _mediator;
        private readonly IColourPalette _colourPalette;
        private readonly ICommandResponder _responder;

        /// <summary>
        /// Instantiates a new instance of <see cref="GeneralModerationCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="responder">The command responder.</param>
        public GeneralModerationCommands(
            MessageContext context,
            IMediator mediator,
            IColourPalette colourPalette,
            ICommandResponder responder
        )
        {
            _context = context;
            _mediator = mediator;
            _colourPalette = colourPalette;
            _responder = responder;
        }

        [Command("view", "v")]
        [Description("Views a moderation action.")]
        public async Task<IResult> View(int id) =>
            await _mediator.Send(new GetById.Query(id, _context.GuildID.Value, false)) switch
            {
                { IsSuccess: true, Entity: { } e } =>
                    await _responder.Respond(new Embed
                    {
                        Title = "Moderation action information",
                        Fields = new List<EmbedField>
                        {
                            new("ID", e.ModerationActionId.ToString(), false),
                            new("Type", e.ModerationActionType.ToStringWithEmoji(), false),
                            new("User's IGN", e.UserIgn ?? "None", false),
                            new("User's Discord",
                                e.UserDiscordId is not null
                                    ? $"<@{e.UserDiscordId}>"
                                    : "None", false)
                        },
                        Colour = e.ModerationActionType switch
                        {
                            ModerationActionType.Ban => _colourPalette.Red,
                            ModerationActionType.Mute => _colourPalette.Pink,
                            ModerationActionType.Warn => _colourPalette.Yellow,
                            _ => new()
                        },
                        Thumbnail = EmbedProperties.MmccLogoThumbnail,
                        Timestamp = DateTimeOffset.UtcNow
                    }),

                { IsSuccess: true } =>
                    Result.FromError(new NotFoundError($"Could not find a moderation action with ID: {id}")),

                { IsSuccess: false } res => res
            };

        /// <summary>
        /// Deactivates a moderation action by ID.
        /// </summary>
        /// <param name="id">ID of the action to deactivate.</param>
        /// <returns>Result of the operation.</returns>
        [Command("deactivate", "disable", "revoke", "cancel")]
        [Description("Deactivates a moderation action.")]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> Deactivate(int id)
        {
            var getAppResult = await _mediator.Send(new GetById.Query(id, _context.GuildID.Value));
            if (!getAppResult.IsSuccess)
            {
                return getAppResult;
            }
            
            Result<ModerationAction> deactivateResult = getAppResult.Entity.ModerationActionType switch
            {
                ModerationActionType.Ban => await _mediator.Send(new Unban.Command
                    { ModerationAction = getAppResult.Entity, ChannelId = _context.ChannelID }),

                _ => Result<ModerationAction>.FromError(new UnsupportedFeatureError("Unsupported moderation type."))
            };
            if (!deactivateResult.IsSuccess)
            {
                return deactivateResult;
            }

            var embed = new Embed
            {
                Title = ":white_check_mark: Moderation action deactivated successfully.",
                Description = "The following moderation action has been deactivated.",
                Fields = new List<EmbedField>
                {
                    new("ID", deactivateResult.Entity.ModerationActionId.ToString(), false),
                    new("Type", deactivateResult.Entity.ModerationActionType.ToStringWithEmoji(), false),
                    new("User's IGN", deactivateResult.Entity.UserIgn ?? "None", false),
                    new("User's Discord",
                        deactivateResult.Entity.UserDiscordId is not null
                            ? $"<@{deactivateResult.Entity.UserDiscordId}>"
                            : "None", false)
                },
                Colour = _colourPalette.Green,
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Timestamp = DateTimeOffset.UtcNow
            };
            return await _responder.Respond(embed);
        }
    }
}