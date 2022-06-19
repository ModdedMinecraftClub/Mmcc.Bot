using System.Threading.Tasks;
using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Mmcc.Bot.Polychat.Errors;
using Remora.Results;

namespace Mmcc.Bot.Polychat.Networking;

/// <summary>
/// Asynchronous handler for Polychat messages.
/// </summary>
///
/// <remarks>
/// Polychat messages are messages that have already been parsed from raw bytes over TCP by <see cref="TcpMessageHandler"/>.
/// </remarks>
public interface IScopedAsyncPolychatMessageHandler
{
    /// <summary>
    /// Handles an incoming Polychat message.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    Task<Result> Handle();
}

/// <inheritdoc />
public class ScopedAsyncPolychatMessageHandler : IScopedAsyncPolychatMessageHandler
{
    private readonly PolychatMessageContext _msgContext;
    private readonly IRequestResolver _requestResolver;
    private readonly IMediator _mediator;
    private readonly TelemetryClient? _telemetry;

    public ScopedAsyncPolychatMessageHandler(
        PolychatMessageContext msgContext,
        IRequestResolver requestResolver,
        IMediator mediator,
        TelemetryClient? telemetry = null
    )
    {
        _msgContext = msgContext;
        _requestResolver = requestResolver;
        _mediator = mediator;
        _telemetry = telemetry;
    }

    /// <inheritdoc />
    public async Task<Result> Handle()
    {
        using var operation = _telemetry?.StartOperation<RequestTelemetry>(_msgContext.GetTelemetryMessageIdentifier());

        if (operation is not null)
        {
            operation.Telemetry.Success = false;
            operation.Telemetry.ResponseCode = PolychatTelemetryCodes.InternalError;
        }

        var req = _requestResolver.Resolve();

        if (req is null)
        {
            if (operation is not null)
            {
                operation.Telemetry.ResponseCode = PolychatTelemetryCodes.UnknownMessage;
            }
            
            return Result.FromError(
                new UnknownMessageType($"Could not find a matching request type for '{_msgContext.RawTypeUrl}'"));
        }

        await _mediator.Send(req);

        if (operation is not null)
        {
            operation.Telemetry.Success = true;
            operation.Telemetry.ResponseCode = PolychatTelemetryCodes.Ok;
        }
        
        return Result.FromSuccess();
    }
}