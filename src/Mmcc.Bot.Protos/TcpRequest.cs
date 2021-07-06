using Google.Protobuf;
using MediatR;
using Ssmp;

namespace Mmcc.Bot.Protos
{
    /// <summary>
    /// Represents a request coming from a TCP client.
    /// </summary>
    /// <param name="ConnectedClient">Request's author.</param>
    /// <param name="Message">Request's body.</param>
    /// <typeparam name="T">Request message type.</typeparam>
    public record TcpRequest<T>(
        ConnectedClient ConnectedClient,
        T Message
    ) : IRequest where T : IMessage<T>;
}