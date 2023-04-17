using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.RemoraAbstractions.Services;
using Mmcc.Bot.RemoraAbstractions.Services.Interactions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.Middleware;

public class InteractionErrorNotificationMiddleware : IInteractionPostExecutionEvent
{
    private readonly ILogger<ErrorNotificationMiddleware> _logger;
    private readonly IErrorProcessingService _errorProcessingService;
    private readonly IInteractionHelperService _interactionHelper;

    public InteractionErrorNotificationMiddleware(
        ILogger<ErrorNotificationMiddleware> logger,
        IErrorProcessingService errorProcessingService, 
        IInteractionHelperService interactionHelper
    )
    {
        _logger = logger;
        _errorProcessingService = errorProcessingService;
        _interactionHelper = interactionHelper;
    }

    public async Task<Result> AfterExecutionAsync(
        InteractionContext interactionContext,
        Result interactionResult,
        CancellationToken ct
    )
    {
        if (interactionResult.IsSuccess)
        {
            return Result.FromSuccess();
        }
        
        var err = interactionResult.Error;
        var errorEmbed = _errorProcessingService.GetErrorEmbed(err);

        // var sendEmbedResult = await _channelApi.CreateMessageAsync(interactionContext.ChannelID, embeds: new[] { errorEmbed }, ct: ct);
        var sendErrorEmbed = await _interactionHelper.SendFollowup(errorEmbed);
        return !sendErrorEmbed.IsSuccess
            ? Result.FromError(sendErrorEmbed.Error)
            : Result.FromSuccess();
    }
}