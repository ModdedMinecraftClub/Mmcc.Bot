using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

using BanModerationAction = Mmcc.Bot.Infrastructure.Commands.ModerationActions.Ban;

namespace Mmcc.Bot.CommandGroups.Moderation
{
    /// <summary>
    /// Commands for banning users.
    /// </summary>
    [Group("ban")]
    [Description("Moderation (bans)")]
    [RequireGuild]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public class BanCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly Embed _embedBase;

        /// <summary>
        /// Instantiates a new instance of <see cref="BanCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public BanCommands(MessageContext context, IDiscordRestChannelAPI channelApi, IMediator mediator, ColourPalette colourPalette)
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _embedBase = new Embed
            {
                Description = "User has been banned successfully.",
                Colour = colourPalette.Green
            };
        }
        
        /// <summary>
        /// Bans a Discord user.
        /// </summary>
        /// <param name="user">Discord user.</param>
        /// <param name="expiryDate">Expiry date.</param>
        /// <param name="reason">Reason.</param>
        /// <returns>Result of the operation.</returns>
        ///
        /// <remarks>This command is for Discord user only. It will not ban from MC servers.</remarks>
        [Command("discord", "d")]
        [Description("Bans a Discord user (Discord only)")]
        public async Task<IResult> BanDiscord(IUser user, ExpiryDate expiryDate, [Greedy] string reason) =>
            await _mediator.Send(new BanModerationAction.Command
                {
                    GuildId = _context.Message.GuildID.Value,
                    ChannelId = _context.ChannelID,
                    UserDiscordId = user.ID,
                    Reason = reason,
                    ExpiryDate = expiryDate.Value,
                    UserIgn = null
                }) switch
                {
                    {IsSuccess: true} => 
                        await _channelApi.CreateMessageAsync(_context.ChannelID, embed: _embedBase with
                        {
                            Title = ":white_check_mark: User banned successfully (Discord only).",
                            Timestamp = DateTimeOffset.UtcNow
                        }),
                    
                    {IsSuccess: false} res => res
                };

        /// <summary>
        /// Bans a MC user.
        /// </summary>
        /// <param name="ign">MC user's IGN.</param>
        /// <param name="expiryDate">Expiry date.</param>
        /// <param name="reason">Reason.</param>
        /// <returns>Result of the operation</returns>
        ///
        /// <remarks>This command is for MC user only. It will not ban from Discord.</remarks>
        [Command("ig")]
        [Description("Bans a user from all MC servers. (In-game only)")]
        public async Task<IResult> BanIg(string ign, ExpiryDate expiryDate, [Greedy] string reason) =>
            await _mediator.Send(new BanModerationAction.Command
                {
                    GuildId = _context.Message.GuildID.Value,
                    ChannelId = _context.ChannelID,
                    UserIgn = ign,
                    Reason = reason,
                    ExpiryDate = expiryDate.Value,
                    UserDiscordId = null
                }) switch
                {
                    {IsSuccess: true} =>
                        await _channelApi.CreateMessageAsync(_context.ChannelID, embed: _embedBase with
                        {
                            Title = ":white_check_mark: User banned successfully from all MC servers (in-game only).",
                            Timestamp = DateTimeOffset.UtcNow
                        }),

                    {IsSuccess: false} res => res
                };

        /// <summary>
        /// Bans a user from Discord and MC servers.
        /// </summary>
        /// <param name="discordUser">Discord user.</param>
        /// <param name="ign">User's MC IGN.</param>
        /// <param name="expiryDate">Expiry date.</param>
        /// <param name="reason">Reason.</param>
        /// <returns>Result of the operation.</returns>
        [Command("all", "a")]
        [Description("Bans the user from both MC servers and Discord")]
        public async Task<IResult> BanAll(IUser discordUser, string ign, ExpiryDate expiryDate, [Greedy] string reason) =>
            await _mediator.Send(new BanModerationAction.Command
                {
                    GuildId = _context.Message.GuildID.Value,
                    ChannelId = _context.ChannelID,
                    UserIgn = ign,
                    Reason = reason,
                    ExpiryDate = expiryDate.Value,
                    UserDiscordId = discordUser.ID
                }) switch
                {
                    {IsSuccess: true} =>
                        await _channelApi.CreateMessageAsync(_context.ChannelID, embed: _embedBase with
                        {
                            Title = ":white_check_mark: User banned successfully from all MC servers and Discord.",
                            Timestamp = DateTimeOffset.UtcNow
                        }),

                    {IsSuccess: false} res => res
                };
    }
}