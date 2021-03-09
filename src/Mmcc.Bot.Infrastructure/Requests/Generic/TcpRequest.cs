using Google.Protobuf;
using MediatR;
using Ssmp;

namespace Mmcc.Bot.Infrastructure.Requests.Generic
{
    /// <summary>
    /// Represents a request coming from a TCP client.
    /// </summary>
    /// <typeparam name="T">Request message type.</typeparam>
    public class TcpRequest<T> : IRequest where T : IMessage<T>
    {
        /// <summary>
        /// Request's author.
        /// </summary>
        public ConnectedClient ConnectedClient { get; }
        
        /// <summary>
        /// Request's message (body).
        /// </summary>
        public T Message { get; }

        /// <summary>
        /// Instantiates a new instance of <see cref="TcpRequest{T}"/>.
        /// </summary>
        /// <param name="connectedClient">Request's author.</param>
        /// <param name="message">Request's message (body).</param>
        public TcpRequest(ConnectedClient connectedClient, T message)
        {
            ConnectedClient = connectedClient;
            Message = message;
        }
    }
}