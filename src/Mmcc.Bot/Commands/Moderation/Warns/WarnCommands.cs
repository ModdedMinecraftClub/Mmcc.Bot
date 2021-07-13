using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.Commands.Moderation.Warns
{
    /// <summary>
    /// Commands for issuing warnings.
    /// </summary>
    [Group("warn")]
    [Description("Moderation (warns)")]
    [RequireGuild]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public class WarnCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IMediator _mediator;
        private readonly Embed _embedBase;
        private readonly ICommandResponder _responder;

        /// <summary>
        /// Instantiates a new instance of <see cref="WarnCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="responder">The command responder.</param>
        public WarnCommands(
            MessageContext context,
            IMediator mediator,
            IColourPalette colourPalette,
            ICommandResponder responder
        )
        {
            _context = context;
            _mediator = mediator;
            _responder = responder;

            _embedBase = new Embed
            {
                Description = "User has been warned successfully.",
                Colour = colourPalette.Green
            };
        }

        [Command("discord", "d")]
        [Description("Warns a Discord user (Discord only)")]
        public async Task<IResult> WarnDiscord(IUser user, [Greedy] string reason) =>
            await _mediator.Send(new Warn.Command
                {
                    UserDiscordId = user.ID,
                    GuildId = _context.Message.GuildID.Value,
                    Reason = reason,
                    UserIgn = null
                }) switch
                {
                    { IsSuccess: true } =>
                        await _responder.Respond(_embedBase with
                        {
                            Title = ":white_check_mark: Discord user has been successfully warned (Discord only).",
                            Timestamp = DateTimeOffset.UtcNow
                        }),

                    { IsSuccess: false } res => res
                };

        [Command("ig")]
        [Description("Warns a player in-game (in-game only).")]
        public async Task<IResult> WarnIg(string ign, [Greedy] string reason) =>
            await _mediator.Send(new Warn.Command
                {
                    UserIgn = ign,
                    GuildId = _context.Message.GuildID.Value,
                    Reason = reason,
                    UserDiscordId = null
                }) switch
                {
                    { IsSuccess: true } =>
                        await _responder.Respond(_embedBase with
                        {
                            Title = ":white_check_mark: In-game user has been successfully warned (In-game only).",
                            Timestamp = DateTimeOffset.UtcNow
                        }),

                    { IsSuccess: false } res => res
                };

        [Command("all", "a")]
        [Description("Warns a player both in-game and on Discord.")]
        public async Task<IResult> WarnAll(IUser discordUser, string ign, [Greedy] string reason)
        {
            return await _mediator.Send
                (
                    new Warn.Command
                    {
                        UserDiscordId = discordUser.ID,
                        UserIgn = ign,
                        GuildId = _context.Message.GuildID.Value,
                        Reason = reason
                    }
                ) switch
                {
                    { IsSuccess: true } =>
                        await _responder.Respond(_embedBase with
                        {
                            Title =
                            ":white_check_mark: User has been successfully warned both in-game and on Discord.",
                            Timestamp = DateTimeOffset.UtcNow
                        }),

                    { IsSuccess: false } res => res
                };
        }
    }
}