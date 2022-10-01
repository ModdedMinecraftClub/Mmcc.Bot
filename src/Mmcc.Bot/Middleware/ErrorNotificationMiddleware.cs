using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Mmcc.Bot.Middleware;

public class ErrorNotificationMiddleware : IPostExecutionEvent
{
    private readonly ILogger<ErrorNotificationMiddleware> _logger;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IErrorProcessingService _errorProcessingService;

    public ErrorNotificationMiddleware(
        ILogger<ErrorNotificationMiddleware> logger,
        IDiscordRestChannelAPI channelApi, 
        IErrorProcessingService errorProcessingService
    )
    {
        _logger = logger;
        _channelApi = channelApi;
        _errorProcessingService = errorProcessingService;
    }

    public async Task<Result> AfterExecutionAsync(
        ICommandContext context,
        IResult executionResult,
        CancellationToken ct
    )
    {
        if (executionResult.IsSuccess)
        {
            return Result.FromSuccess();
        }

        var err = executionResult.Error;
        var errorEmbed = _errorProcessingService.GetErrorEmbed(err);

        var sendEmbedResult =
            await _channelApi.CreateMessageAsync(context.ChannelID, embeds: new[] { errorEmbed }, ct: ct);
        return !sendEmbedResult.IsSuccess
            ? Result.FromError(sendEmbedResult)
            : Result.FromSuccess();
    }
}