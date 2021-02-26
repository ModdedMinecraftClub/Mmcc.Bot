using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Minecraft
{
    /// <summary>
    /// Commands for managing MC servers.
    /// </summary>
    [Group("mc")]
    public class MinecraftServersCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;

        public MinecraftServersCommands(MessageContext context, IDiscordRestChannelAPI channelApi, IMediator mediator)
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
        }

        /// <summary>
        /// Shows info about online servers.
        /// </summary>
        /// <returns>Result of the operation.</returns>
        /// <exception cref="NotImplementedException"></exception>
        [Command("online", "o")]
        [Description("Shows info about online servers")]
        [RequireGuild]
        public async Task<IResult> Online()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shows current TPS of a MC server.
        /// </summary>
        /// <param name="serverId">ID of the server.</param>
        /// <returns>Result of the operation.</returns>
        /// <exception cref="NotImplementedException"></exception>
        [Command("tps")]
        [Description("Shows current TPS of a MC server")]
        [RequireGuild]
        public async Task<IResult> Tps(string serverId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes a command on a MC server.
        /// </summary>
        /// <param name="serverId">ID of the server.</param>
        /// <param name="command">MC command to execute.</param>
        /// <param name="args">Command arguments.</param>
        /// <returns>Result of the operation.</returns>
        /// <exception cref="NotImplementedException"></exception>
        [Command("exec", "e", "execute")]
        [Description("Executes a command on a MC server")]
        [RequireGuild]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> Exec(string serverId, string command, [Greedy] IEnumerable<string> args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Restarts a MC server.
        /// </summary>
        /// <param name="serverId">ID of the server to restart.</param>
        /// <returns>Result of the operation.</returns>
        /// <exception cref="NotImplementedException"></exception>
        [Command("restart", "r")]
        [Description("Restarts a server")]
        [RequireGuild]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> Restart(string serverId)
        {
            throw new NotImplementedException();
        }
    }
}