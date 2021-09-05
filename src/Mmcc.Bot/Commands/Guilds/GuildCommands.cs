using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Commands.Guilds
{
    /// <summary>
    /// Core commands.
    /// </summary>
    [RequireGuild]
    public class GuildCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IColourPalette _colourPalette;
        private readonly IMediator _mediator;
        private readonly ICommandResponder _responder;

        /// <summary>
        /// Instantiates a new instance of <see cref="GuildCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="responder">The command responder.</param>
        public GuildCommands(
            MessageContext context,
            IColourPalette colourPalette,
            IMediator mediator,
            ICommandResponder responder
        )
        {
            _context = context;
            _colourPalette = colourPalette;
            _mediator = mediator;
            _responder = responder;
        }

        [Command("guild")]
        [Description("Provides information about the current guild.")]
        public async Task<IResult> GuildInfo() =>
            await _mediator.Send(new GetGuildInfo.Query(_context.GuildID.Value)) switch
            {
                { IsSuccess: true, Entity: { } e } =>
                    await _responder.Respond(new Embed
                    {
                        Title = "Guild info",
                        Description = "Information about the current guild.",
                        Fields = new List<EmbedField>
                        {
                            new("Name", e.GuildName, false),
                            new("Owner", $"<@{e.GuildOwnerId}>"),
                            new("Max members", e.GuildMaxMembers.ToString() ?? "Unavailable", false),
                            new("Available roles", string.Join(", ", e.GuildRoles.Select(r => $"<@&{r.ID}>")))
                        },
                        Timestamp = DateTimeOffset.UtcNow,
                        Colour = _colourPalette.Blue,
                        Thumbnail = e.GuildIconUrl is null
                            ? new Optional<IEmbedThumbnail>()
                            : new EmbedThumbnail(e.GuildIconUrl.ToString())
                    }),

                { IsSuccess: true } =>
                    Result.FromError(new NotFoundError($"Guild with ID: {_context.GuildID.Value} not found")),

                { IsSuccess: false } res => res,
            };

        [Command("invite")]
        [Description("Gives an invite link to the current guild.")]
        public async Task<IResult> Invite() =>
            await _mediator.Send(new GetInviteLink.Query(_context.GuildID.Value)) switch
            {
                {IsSuccess: true, Entity: { } e} =>
                    await _responder.Respond($"https://discord.gg/{e}"),

                {IsSuccess: true} => Result.FromError(new NotFoundError("Could not find invite link for this guild.")),

                {IsSuccess: false} res => res
            };
    }
}