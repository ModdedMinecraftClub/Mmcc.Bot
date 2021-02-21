using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Moderation
{
    /// <summary>
    /// Commands for obtaining information about players.
    /// </summary>
    [Group("info")]
    [Description("Information about players")]
    public class InfoCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;
        
        /// <summary>
        /// Instantiates a new instance of <see cref="InfoCommands"/>.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public InfoCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            IMediator mediator,
            ColourPalette colourPalette
        )
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _colourPalette = colourPalette;
        }
        
        /// <summary>
        /// Views info about a player by IGN.
        /// </summary>
        /// <param name="ign">IGN.</param>
        /// <returns>The result of the operation.</returns>
        [Command("info ig")]
        [Description("Obtains information about a player by IGN")]
        [RequireGuild]
        public async Task<IResult> InfoIg(string ign)
        {
            throw new NotImplementedException();
        }
    }
}