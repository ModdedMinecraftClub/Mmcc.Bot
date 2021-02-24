using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Ban = Mmcc.Bot.Infrastructure.Commands.ModerationActions.Ban;

namespace Mmcc.Bot.CommandGroups.Moderation
{
    /// <summary>
    /// Commands for banning users.
    /// </summary>
    [Group("ban")]
    public class BanCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        
        /// <summary>
        /// Instantiates a new instance of <see cref="BanCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        public BanCommands(MessageContext context, IDiscordRestChannelAPI channelApi, IMediator mediator)
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
        }

        [Command("discord", "d")]
        [Description("Bans a Discord user (Discord only)")]
        [RequireGuild]
        public async Task<IResult> BanDiscord(IUser user, string expiryDate, [Greedy] string reason)
        {
            var commandResult = await _mediator.Send
            (
                new Ban.Command
                {
                    GuildId = _context.Message.GuildID.Value,
                    ChannelId = _context.ChannelID,
                    UserDiscordId = user.ID,
                    Reason = reason,
                    ExpiryDate = null,
                    UserIgn = null
                }
            );
            if (!commandResult.IsSuccess)
            {
                return Result.FromError(commandResult.Error);
            }

            var embed = new Embed
            {
                Title = ":white_check_mark: User banned successfully (Discord only).",
                Description = "User has been banned successfully.",
                Timestamp = DateTimeOffset.UtcNow
            };
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }

        [Command("ig")]
        [Description("Bans a user from all MC servers. (In-game only)")]
        [RequireGuild]
        public async Task<IResult> BanIg(string ign, string expiryDate, [Greedy] string reason)
        {
            var commandResult = await _mediator.Send
            (
                new Ban.Command
                {
                    GuildId = _context.Message.GuildID.Value,
                    ChannelId = _context.ChannelID,
                    UserIgn = ign,
                    Reason = reason,
                    ExpiryDate = null,
                    UserDiscordId = null
                }
            );
            if (!commandResult.IsSuccess)
            {
                return Result.FromError(commandResult.Error);
            }

            var embed = new Embed
            {
                Title = ":white_check_mark: User banned successfully from all MC servers (in-game only).",
                Description = "User has been banned successfully.",
                Timestamp = DateTimeOffset.UtcNow
            };
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
    }
}