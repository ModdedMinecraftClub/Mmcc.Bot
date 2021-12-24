using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.EventResponders.Buttons;

public class TestHandler
{
    public record Command(string InteractionToken, Context Context) : IRequest<Result>;

    public record Context(Snowflake Id);
    
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IInteractionResponder _responder;
        
        public Handler(IInteractionResponder responder)
        {
            _responder = responder;
        }
        
        public async Task<Result> Handle(Command req, CancellationToken ct)
        {
            var msg = JsonSerializer.Serialize(req.Context);

            var respondRes = await _responder.SendFollowup(req.InteractionToken, msg);
            return respondRes.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(respondRes.Error);
        }
    }
}