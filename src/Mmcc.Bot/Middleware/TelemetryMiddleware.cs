using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Mmcc.Bot.Common.Extensions.Remora.Errors;
using Remora.Commands.Results;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Mmcc.Bot.Middleware;

public class TelemetryMiddleware : IPostExecutionEvent
{
    private readonly TelemetryClient _telemetryClient;

    public TelemetryMiddleware(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct)
    {
        var timeNow = DateTimeOffset.UtcNow;
        // safe cast because we're not using slash commands so always will be a MessageContext;
        var messageContext = (MessageContext) context;
        var messageTimestamp = messageContext.Message.Timestamp;
        var timeInfo = messageTimestamp.HasValue
            ? new {StartTime = messageTimestamp.Value, Duration = timeNow - messageTimestamp.Value}
            : new {StartTime = timeNow - TimeSpan.FromMilliseconds(1), Duration = TimeSpan.FromMilliseconds(1)};
        var reqId = messageContext.Message.Content.HasValue
            ? $"Discord::Command::{messageContext.Message.Content.Value.Split(" ")[0][1..]}"
            : "Discord::Command::Unknown";

        if (commandResult.IsSuccess || commandResult.Error is CommandNotFoundError)
        {
            _telemetryClient.TrackRequest(reqId, timeInfo.StartTime, timeInfo.Duration, "OK", true);
        }
        else if (commandResult.Error is not null)
        {
            _telemetryClient.TrackRequest(
                reqId,
                timeInfo.StartTime,
                timeInfo.Duration,
                commandResult.Error.GetType().ToString(),
                false);
            _telemetryClient.TrackException(commandResult.Error.GetException(), commandResult.Error.ToDictionary());
        }
        else
        {
            _telemetryClient.TrackRequest(
                reqId,
                timeInfo.StartTime,
                timeInfo.Duration,
                "ERR_UNSPECIFIED",
                false);
        }

        return Task.FromResult(Result.FromSuccess());
    }
}