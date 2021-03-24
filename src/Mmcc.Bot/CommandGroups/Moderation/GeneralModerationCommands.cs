using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Extensions.Database.Entities;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries.ModerationActions;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Moderation
{
    /// <summary>
    /// General moderation commands that do not fit into any specific categories.
    /// </summary>
    [Group("moderation", "mod")]
    [RequireGuild]
    public class GeneralModerationCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;
        private readonly IModerationService _moderationService;

        /// <summary>
        /// Instantiates a new instance of <see cref="GeneralModerationCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="moderationService">The moderation service.</param>
        public GeneralModerationCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            IMediator mediator,
            ColourPalette colourPalette,
            IModerationService moderationService
        )
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _colourPalette = colourPalette;
            _moderationService = moderationService;
        }

        [Command("view", "v")]
        [Description("Views a moderation action.")]
        public async Task<IResult> View(int id)
        {
            var getAppResult =
                await _mediator.Send(new GetById.Query(ModerationActionId: id, GuildId: _context.GuildID.Value,
                    EnableTracking: false));
            
            if (!getAppResult.IsSuccess)
            {
                return getAppResult;
            }
            if (getAppResult.Entity is null)
            {
                return Result.FromError(
                    new NotFoundError($"Could not find application with ID {id} that belongs to current guild."));
            }
            
            var embed = new Embed
            {
                Title = "Moderation action information",
                Fields = new List<EmbedField>
                {
                    new("ID", getAppResult.Entity.ModerationActionId.ToString(), false),
                    new("Type", getAppResult.Entity.ModerationActionType.ToStringWithEmoji(), false),
                    new("User's IGN", getAppResult.Entity.UserIgn ?? "None", false),
                    new("User's Discord",
                        getAppResult.Entity.UserDiscordId is not null
                            ? $"<@{getAppResult.Entity.UserDiscordId}>"
                            : "None", false)
                },
                Timestamp = DateTimeOffset.UtcNow,
                Colour = _colourPalette.Green
            };
            return await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed); 
        }

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
            if (getAppResult.Entity is null)
            {
                return Result.FromError(
                    new NotFoundError($"Could not find application with ID {id} that belongs to current guild."));
            }

            var deactivateResult = await _moderationService.Deactivate(getAppResult.Entity, _context.ChannelID);

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
                Timestamp = DateTimeOffset.UtcNow,
                Colour = _colourPalette.Green
            };
            return await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed); 
        }
    }
}