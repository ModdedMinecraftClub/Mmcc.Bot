using Google.Protobuf;
using MediatR;
using Ssmp;

namespace Mmcc.Bot.Polychat.Networking;

/// <summary>
/// Represents a request coming from a Polychat client.
/// </summary>
/// <param name="ConnectedClient">Request's author.</param>
/// <param name="Message">Request's body.</param>
/// <typeparam name="T">Request message type.</typeparam>
public record PolychatRequest<T>(
    ConnectedClient ConnectedClient,
    T Message
) : IPolychatRequest where T : IMessage<T>;

public interface IPolychatRequest : IRequest
{
    public ConnectedClient ConnectedClient { get; init; }
}