using System;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Infrastructure.Commands.ModerationActions;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Services
{
    /// <summary>
    /// Moderation service.
    /// </summary>
    public interface IModerationService
    {
        /// <summary>
        /// Deactivates a moderation action.
        /// </summary>
        /// <param name="moderationAction">Moderation action to deactivate.</param>
        /// <param name="channelId">Channel ID to which polychat2 will send the notification if needed.</param>
        /// <returns>Result of the operation.</returns>
        Task<Result> Deactivate(ModerationAction moderationAction, Snowflake channelId);
    }
    
    /// <inheritdoc />
    public class ModerationService : IModerationService
    {
        private readonly IMediator _mediator;
        
        /// <summary>
        /// Instantiates a new instance of <see cref="ModerationAction"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        public ModerationService(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <inheritdoc />
        public async Task<Result> Deactivate(ModerationAction moderationAction, Snowflake channelId)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (moderationAction.ModerationActionType)
            {
                case ModerationActionType.Ban:
                    var command = await _mediator.Send(new Unban.Command
                        {ModerationAction = moderationAction, ChannelId = channelId});
                    
                    if (!command.IsSuccess)
                    {
                        return Result.FromError(command);
                    }
                    
                    break;
                case ModerationActionType.Mute:
                    throw new NotImplementedException();
                    break;
                default:
                    return new GenericError("Unsupported moderation type.");
            }
            
            return Result.FromSuccess();
        }
    }
}