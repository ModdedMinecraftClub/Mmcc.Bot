using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Infrastructure.Requests.Generic;
using Mmcc.Bot.Protos;

namespace Mmcc.Bot.Infrastructure.Commands.Polychat.IncomingMessageHandlers
{
    public class HandlePromoteMemberCommand
    {
        public class Handler : AsyncRequestHandler<TcpRequest<PromoteMemberCommand>>
        {
            protected override Task Handle(TcpRequest<PromoteMemberCommand> request, CancellationToken cancellationToken)
            {
                Console.WriteLine(nameof(HandlePromoteMemberCommand));
                return Task.CompletedTask;
            }
        }
    }
}