﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Infrastructure.Commands.ModerationActions;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
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
    /// Commands for issuing warnings.
    /// </summary>
    [Group("warn")]
    public class WarnCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly Embed _embedBase;
        
        /// <summary>
        /// Instantiates a new instance of <see cref="WarnCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public WarnCommands(MessageContext context, IDiscordRestChannelAPI channelApi, IMediator mediator, ColourPalette colourPalette)
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _embedBase = new Embed
            {
                Description = "User has been warned successfully.",
                Colour = colourPalette.Green
            };
        }

        [Command("discord", "d")]
        [Description("Warns a Discord user (Discord only)")]
        [RequireGuild]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> WarnDiscord(IUser user, [Greedy] string reason)
        {
            var commandResult = await _mediator.Send
            (
                new Warn.Command
                {
                    UserDiscordId = user.ID,
                    GuildId = _context.Message.GuildID.Value,
                    Reason = reason,
                    UserIgn = null
                }
            );
            if (!commandResult.IsSuccess)
            {
                return Result.FromError(commandResult.Error);
            }

            var embed = _embedBase with
            {
                Title = ":white_check_mark: Discord user has been successfully warned (Discord only).",
                Timestamp = DateTimeOffset.UtcNow
            };
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }

        [Command("ig")]
        [Description("Warns a player in-game (in-game only).")]
        [RequireGuild]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> WarnIg(string ign, [Greedy] string reason)
        {
            var commandResult = await _mediator.Send
            (
                new Warn.Command
                {
                    UserIgn = ign,
                    GuildId = _context.Message.GuildID.Value,
                    Reason = reason,
                    UserDiscordId = null
                }
            );
            if (!commandResult.IsSuccess)
            {
                return Result.FromError(commandResult.Error);
            }

            var embed = _embedBase with
            {
                Title = ":white_check_mark: In-game user has been successfully warned (In-game only).",
                Timestamp = DateTimeOffset.UtcNow
            };
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }

        [Command("all", "a")]
        [Description("Warns a player both in-game and on Discord.")]
        [RequireGuild]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> WarnAll(IUser discordUser, string ign, [Greedy] string reason)
        {
            var commandResult = await _mediator.Send
            (
                new Warn.Command
                {
                    UserDiscordId = discordUser.ID,
                    UserIgn = ign,
                    GuildId = _context.Message.GuildID.Value,
                    Reason = reason
                }
            );
            if (!commandResult.IsSuccess)
            {
                return Result.FromError(commandResult.Error);
            }

            var embed = _embedBase with
            {
                Title = ":white_check_mark: User has been successfully warned both in-game and on Discord.",
                Timestamp = DateTimeOffset.UtcNow
            };
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
    }
}