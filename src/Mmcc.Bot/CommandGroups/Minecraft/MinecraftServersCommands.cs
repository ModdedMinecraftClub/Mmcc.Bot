using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Infrastructure.Commands.Polychat.MessageSenders;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.RemoraAbstractions;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Minecraft
{
    /// <summary>
    /// Commands for managing MC servers.
    /// </summary>
    [Group("mc")]
    [Description("Minecraft (Polychat)")]
    [RequireGuild]
    public class MinecraftServersCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IMediator _mediator;
        private readonly IColourPalette _colourPalette;
        private readonly IPolychatService _polychatService;
        private readonly ICommandResponder _responder;

        /// <summary>
        /// Instantiates a new instance of <see cref="MinecraftServersCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="polychatService">The polychat service.</param>
        /// <param name="responder">The command responder.</param>
        public MinecraftServersCommands(
            MessageContext context,
            IMediator mediator,
            IColourPalette colourPalette,
            IPolychatService polychatService,
            ICommandResponder responder
        )
        {
            _context = context;
            _mediator = mediator;
            _colourPalette = colourPalette;
            _polychatService = polychatService;
            _responder = responder;
        }

        /// <summary>
        /// Shows current TPS of a MC server.
        /// </summary>
        /// <param name="serverId">ID of the server.</param>
        /// <returns>Result of the operation.</returns>
        [Command("tps")]
        [Description("Shows current TPS of a MC server")]
        public async Task<IResult> Tps(string serverId) =>
            await _mediator.Send(new SendTpsCommand.Command(serverId, _context.ChannelID));

        /// <summary>
        /// Executes a command on a MC server.
        /// </summary>
        /// <param name="serverId">ID of the server.</param>
        /// <param name="args">Command arguments.</param>
        /// <returns>Result of the operation.</returns>
        [Command("exec", "e", "execute")]
        [Description("Executes a command on a MC server")]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> Exec(string serverId, [Greedy] IEnumerable<string> args) =>
            await _mediator.Send(new SendExecCommand.Command(serverId, _context.ChannelID, args));

        /// <summary>
        /// Restarts a MC server.
        /// </summary>
        /// <param name="serverId">ID of the server to restart.</param>
        /// <returns>Result of the operation.</returns>
        [Command("restart", "r")]
        [Description("Restarts a server")]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> Restart(string serverId) =>
            await _mediator.Send(new SendRestartCommand.Command(serverId, _context.ChannelID));

        /// <summary>
        /// Shows info about online servers.
        /// </summary>
        /// <returns>Result of the operation.</returns>
        [Command("online", "o")]
        [Description("Shows info about online servers")]
        public async Task<IResult> Online()
        {
            var serversInformation = _polychatService.GetInformationAboutOnlineServers().ToList();
            var totalOnlinePlayers = 0;
            var fields = new List<EmbedField>();

            foreach (var serverInformation in serversInformation)
            {
                totalOnlinePlayers += serverInformation.PlayersOnline;

                var fieldName =
                    $"[{serverInformation.ServerId}] {serverInformation.ServerName} [{serverInformation.PlayersOnline}/{serverInformation.MaxPlayers}]";
                var fieldValueSb = new StringBuilder();
                
                fieldValueSb.AppendLine($"*{serverInformation.ServerAddress}*");

                if (serverInformation.OnlinePlayerNames.Any())
                {
                    fieldValueSb.AppendLine(string.Join(", ", serverInformation.OnlinePlayerNames));
                }

                fields.Add(new(fieldName, fieldValueSb.ToString(), false));
            }
            
            var description =
                $"**Servers online:** {serversInformation.Count}\n**Total players online:** {totalOnlinePlayers}";
            var embed = new Embed
            {
                Title = "Online servers",
                Description = description,
                Colour = _colourPalette.Green,
                Timestamp = DateTimeOffset.UtcNow,
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Fields = fields
            };
            return await _responder.Respond(embed);
        }
    }
}