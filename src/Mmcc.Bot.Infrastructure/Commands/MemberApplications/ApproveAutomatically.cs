using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
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
        public class Command : IRequest<Result<MemberApplication>>
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
            /// ID of the channel.
            /// </summary>
            public Snowflake ChannelId { get; set; }

            /// <summary>
            /// Prefix of server.
            /// </summary>
            public string ServerPrefix { get; set; } = null!;
            
            /// <summary>
            /// IGN(s) of the player(s).
            /// </summary>
            public IList<string> Igns { get; set; } = null!;
        }

        /// <summary>
        /// Validates the <see cref="Command"/>.
        /// </summary>
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Id)
                    .NotNull();

                RuleFor(c => c.ChannelId)
                    .NotNull();

                RuleFor(c => c.ServerPrefix)
                    .NotEmpty()
                    .MinimumLength(2);

                RuleFor(c => c.Igns)
                    .NotEmpty();

                RuleForEach(c => c.Igns)
                    .NotEmpty();
            }
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Command, Result<MemberApplication>>
        {
            private readonly BotContext _context;
            private readonly IDiscordRestGuildAPI _guildApi;
            private readonly IPolychatService _ps;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/>.
            /// </summary>
            /// <param name="context">The db context.</param>
            /// <param name="guildApi">The guild API.</param>
            /// <param name="ps">The polychat service.</param>
            public Handler(BotContext context, IDiscordRestGuildAPI guildApi, IPolychatService ps)
            {
                _context = context;
                _guildApi = guildApi;
                _ps = ps;
            }
            
            /// <inheritdoc />
            public async Task<Result<MemberApplication>> Handle(Command request, CancellationToken cancellationToken)
            {
                var app = await _context.MemberApplications
                    .FirstOrDefaultAsync(
                        a => a.MemberApplicationId == request.Id && a.GuildId == request.GuildId.Value,
                        cancellationToken);
                
                if (app is null)
                {
                    return new NotFoundError($"Application with ID `{request.Id}` does not exist");
                }

                var getRoles = await _guildApi.GetGuildRolesAsync(request.GuildId, cancellationToken);
                if (!getRoles.IsSuccess)
                {
                    return Result<MemberApplication>.FromError(getRoles.Error);
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
                    return Result<MemberApplication>.FromError(getUserToPromoteResult.Error);
                }
                if (getUserToPromoteResult.Entity is null)
                {
                    return new NotFoundError($"Could not find a user with ID: `{app.AuthorDiscordId}`.");
                }

                foreach (var ign in request.Igns)
                {
                    var proto = new GenericCommand
                    {
                        DiscordChannelId = request.ChannelId.ToString(),
                        DiscordCommandName = "promote",
                        DefaultCommand = "ranks add $1 member",
                        Args = { ign }
                    };
                    var id = request.ServerPrefix.ToUpperInvariant();
                    var server = _ps.GetOnlineServerOrDefault(id);
                    
                    if (server is null)
                    {
                        return Result<MemberApplication>.FromError(
                            new NotFoundError($"Could not find server with ID {id}"));
                    }

                    await _ps.SendMessage(server, proto);
                }
                
                var addRoleResult =
                    await _guildApi.AddGuildMemberRoleAsync(request.GuildId, userId, role.ID, cancellationToken);
                if (!addRoleResult.IsSuccess)
                {
                    return Result<MemberApplication>.FromError(addRoleResult);
                }

                app.AppStatus = ApplicationStatus.Approved;
                await _context.SaveChangesAsync(cancellationToken);
                
                return Result<MemberApplication>.FromSuccess(app);
            }
        }
    }
}