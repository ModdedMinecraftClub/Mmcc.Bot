using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.MemberApplications
{
    /// <summary>
    /// Approves an application automatically, that is uses polychat2 to approve in-game and bot to approve on Discord.
    /// </summary>
    public class ApproveAutomatically
    {
        /// <summary>
        /// Command to approve an application automatically.
        /// </summary>
        public class Command : IRequest<Result>
        {
            /// <summary>
            /// ID of the application to approve.
            /// </summary>
            public int Id { get; set; }
            
            /// <summary>
            /// ID of the Guild.
            /// </summary>
            public Snowflake GuildId { get; set; }

            /// <summary>
            /// Prefix of server.
            /// </summary>
            public string ServerPrefix { get; set; } = null!;
            
            /// <summary>
            /// IGN(s) of the player(s).
            /// </summary>
            public IList<string> Igns { get; set; } = null!;
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly BotContext _context;
            private readonly IDiscordRestGuildAPI _guildApi;
            private readonly IPolychatCommunicationService _polychatCommunicationService;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/>.
            /// </summary>
            /// <param name="context">The db context.</param>
            /// <param name="guildApi">The guild API.</param>
            /// <param name="polychatCommunicationService">The polychat communication service.</param>
            public Handler(BotContext context, IDiscordRestGuildAPI guildApi, IPolychatCommunicationService polychatCommunicationService)
            {
                _context = context;
                _guildApi = guildApi;
                _polychatCommunicationService = polychatCommunicationService;
            }
            
            /// <inheritdoc />
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                MemberApplication? app;

                try
                {
                    app = await _context.MemberApplications
                        .FirstOrDefaultAsync(
                            a => a.MemberApplicationId == request.Id && a.GuildId == request.GuildId.Value,
                            cancellationToken);
                }
                catch (Exception e)
                {
                    return e;
                }
                
                if (app is null)
                {
                    return new NotFoundError($"Application with ID `{request.Id}` does not exist");
                }

                var getRoles = await _guildApi.GetGuildRolesAsync(request.GuildId, cancellationToken);
                if (!getRoles.IsSuccess)
                {
                    return Result.FromError(getRoles.Error);
                }

                var role = getRoles.Entity
                    .FirstOrDefault(r =>
                        r.Name.Contains($"[{request.ServerPrefix.ToUpper()}]"));
                if (role is null)
                {
                    return new NotFoundError(
                        $"Could not find a role corresponding to the server prefix: `{request.ServerPrefix}`.");
                }

                var userId = new Snowflake(app.AuthorDiscordId);
                var getUserToPromoteResult =
                    await _guildApi.GetGuildMemberAsync(request.GuildId, userId, cancellationToken);
                if (!getUserToPromoteResult.IsSuccess)
                {
                    return Result.FromError(getUserToPromoteResult.Error);
                }
                if (getUserToPromoteResult.Entity is null)
                {
                    return new NotFoundError($"Could not find a user with ID: `{app.AuthorDiscordId}`.");
                }

                foreach (var ign in request.Igns)
                {
                    var polychatProtoMsg = new PromoteMemberCommand
                    {
                        ServerId = request.ServerPrefix,
                        Username = ign
                    };

                    var sendPolychatProtoMsgResult =
                        await _polychatCommunicationService.SendProtobufMessage(polychatProtoMsg);
                    if (!sendPolychatProtoMsgResult.IsSuccess)
                    {
                        return new PolychatError(
                            "Could not communicate with polychat2's central server. Please see the logs.");
                    }
                }
                
                var addRoleResult =
                    await _guildApi.AddGuildMemberRoleAsync(request.GuildId, userId, role.ID, cancellationToken);
                if (!addRoleResult.IsSuccess)
                {
                    return Result.FromError(addRoleResult);
                }

                try
                {
                    app.AppStatus = ApplicationStatus.Approved;
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    return e;
                }
                
                return Result.FromSuccess();
            }
        }
    }
}