using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Infrastructure.Requests.Generic;
using Mmcc.Bot.Protos;

namespace Mmcc.Bot.Infrastructure.Commands.TcpRequests
{
    public class HandleChatMessage
    {
        public class Handler : AsyncRequestHandler<TcpRequest<ChatMessage>>
        {
            protected override Task Handle(TcpRequest<ChatMessage> request, CancellationToken cancellationToken)
            {
                Console.WriteLine(nameof(HandleChatMessage));
                return Task.CompletedTask;
            }
        }
    }
}