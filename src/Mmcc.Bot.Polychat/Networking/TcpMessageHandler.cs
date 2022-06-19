using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ssmp;

namespace Mmcc.Bot.Polychat.Networking;

/// <summary>
/// Service for handling the incoming TCP messages via Ssmp.
/// </summary>
public class TcpMessageHandler : ISsmpHandler
{
    private readonly ILogger<TcpMessageHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TcpMessageHandler(ILogger<TcpMessageHandler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public async ValueTask Handle(ConnectedClient client, byte[] message)
    {
        // create a scope for the incoming message;
        using var msgScope = _scopeFactory.CreateScope();
        
        // set context for the scope;
        var context = msgScope.ServiceProvider.GetRequiredService<PolychatMessageContext>();
        context.Author = client;
        context.MessageContent = Any.Parser.ParseFrom(message);
        
        // handle;
        var scopedAsyncHandler = msgScope.ServiceProvider.GetRequiredService<IScopedAsyncPolychatMessageHandler>();
        var res = await scopedAsyncHandler.Handle();

        if (!res.IsSuccess)
        {
            _logger.LogError("Error while handling a Polychat message: {ErrorType}, {ErrorMsg}", res.Error.GetType(),
                res.Error.Message);
        }
    }
}