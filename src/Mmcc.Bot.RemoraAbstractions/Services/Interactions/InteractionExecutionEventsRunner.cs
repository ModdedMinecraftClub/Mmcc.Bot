using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services.Interactions;

public interface IInteractionExecutionEventsRunner
{
    Task<Result> RunPostExecutionEvents(
        InteractionContext interactionContext,
        Result interactionResult,
        CancellationToken ct
    );
}

public interface IInteractionPostExecutionEvent
{
    Task<Result> AfterExecutionAsync(
        InteractionContext interactionContext,
        Result interactionResult,
        CancellationToken ct = default
    );
}

public class InteractionExecutionEventsRunner : IInteractionExecutionEventsRunner
{
    private readonly IEnumerable<IInteractionPostExecutionEvent> _events;

    public InteractionExecutionEventsRunner(IEnumerable<IInteractionPostExecutionEvent> events)
        => _events = events;

    public async Task<Result> RunPostExecutionEvents(
        InteractionContext interactionContext,
        Result interactionResult,
        CancellationToken ct
    )
    {
        var results = await Task.WhenAll(
            _events.Select(x => x.AfterExecutionAsync(interactionContext, interactionResult, ct))
        );

        foreach (var result in results)
        {
            if (!result.IsSuccess)
            {
                return result;
            }
        }
        
        return Result.FromSuccess();
    }
}