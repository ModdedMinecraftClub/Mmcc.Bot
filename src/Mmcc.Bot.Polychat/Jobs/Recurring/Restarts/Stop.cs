using System;
using Hangfire;
using MediatR;
using Remora.Results;

namespace Mmcc.Bot.Polychat.Jobs.Recurring.Restarts;

public class Stop
{
    public record Command(string ServerId) : IRequest<Result>;
    
    public class Handler : RequestHandler<Command, Result>
    {
        protected override Result Handle(Command request)
        {
            var (serverId) = request;
            var jobId = AutoServerRestartJob.CreateJobId(serverId);

            try
            {
                RecurringJob.RemoveIfExists(jobId);
            }
            catch (Exception e)
            {
                return e;
            }

            return Result.FromSuccess();
        }
    }
}