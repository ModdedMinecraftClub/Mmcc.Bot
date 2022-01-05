using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Polychat.Models;
using Mmcc.Bot.Polychat.Services;
using Remora.Results;

namespace Mmcc.Bot.Polychat.Jobs.Recurring.Restarts;

public class NotifyAboutRestart
{
    public record Command(OnlineServer ServerToNotify, TimeSpan TimeUntilRestart) : IRequest<Result>;
    
    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IPolychatService _ps;

        public Handler(IPolychatService ps) 
            => _ps = ps;

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var (serverToNotify, timeUntilRestart) = request;
            var timespanString = timeUntilRestart > TimeSpan.FromMinutes(1)
                ? timeUntilRestart.ToString()
                : $"{timeUntilRestart.TotalSeconds}s";
            var protoMsgContent = $"§d[AutoRestart] §rServer restarting in {timespanString}.";
            var protoMsg = new ChatMessage
            {
                ServerId = "Discord",
                Message = protoMsgContent,
                MessageOffset = protoMsgContent.IndexOf('S')
            };

            try
            {
                await _ps.SendMessage(serverToNotify, protoMsg);
            }
            catch(Exception e)
            {
                return e;
            }

            return Result.FromSuccess();
        }
    }
}