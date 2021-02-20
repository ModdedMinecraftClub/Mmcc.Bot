using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.MemberApplications
{
    /// <summary>
    /// Rejects a member application.
    /// </summary>
    public class Reject
    {
        /// <summary>
        /// Command to reject an application.
        /// </summary>
        public class Command : IRequest<Result<MemberApplication>>
        {
            /// <summary>
            /// ID of the application to reject.
            /// </summary>
            public int Id { get; set; }
            
            /// <summary>
            /// ID of the channel in which the command was executed.
            /// </summary>
            public Snowflake GuildId { get; set; }
            
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Command, Result<MemberApplication>>
        {
            private readonly BotContext _context;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/>.
            /// </summary>
            /// <param name="context">The db context.</param>
            public Handler(BotContext context)
            {
                _context = context;
            }

            /// <inheritdoc />
            public async Task<Result<MemberApplication>> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var app = await _context.MemberApplications
                        .FirstOrDefaultAsync(
                            a => a.MemberApplicationId == request.Id && a.GuildId == request.GuildId.Value,
                            cancellationToken);
                    if (app is null)
                    {
                        return new NotFoundError($"Could not find application with ID: {request.Id}");
                    }

                    app.AppStatus = ApplicationStatus.Rejected;
                    await _context.SaveChangesAsync(cancellationToken);
                    
                    return Result<MemberApplication>.FromSuccess(app);
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}