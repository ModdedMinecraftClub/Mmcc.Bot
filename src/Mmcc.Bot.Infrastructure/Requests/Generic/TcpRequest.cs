using Google.Protobuf;
using MediatR;
using Ssmp;

namespace Mmcc.Bot.Infrastructure.Requests.Generic
{
    public class TcpRequest<T> : IRequest where T : IMessage<T>
    {
        public ConnectedClient ConnectedClient { get; }
        public T? Message { get; }

        public TcpRequest(ConnectedClient connectedClient, T? message)
        {
            ConnectedClient = connectedClient;
            Message = message;
        }
    }
}