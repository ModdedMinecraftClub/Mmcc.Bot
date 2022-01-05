using System.Linq;
using Hangfire;
using Hangfire.Storage;
using MediatR;
using Mmcc.Bot.Polychat.Jobs.Recurring.Restarts;
using Remora.Results;

namespace Mmcc.Bot.Commands.Minecraft.Restarts;

public class ScheduleOrUpdate
{
    public record Query(string ServerId, string CronExpression) : IRequest<Result<RecurringJobDto?>>;

    public class Handler : RequestHandler<Query, Result<RecurringJobDto?>>
    {
        protected override Result<RecurringJobDto?> Handle(Query request)
        {
            var (serverId, cronExpression) = request;
            var jobId = AutoServerRestartJob.CreateJobId(serverId);
            
            RecurringJob.AddOrUpdate<AutoServerRestartJob>(jobId, job => job.Execute(serverId), cronExpression);

            return JobStorage.Current
                .GetConnection()
                .GetRecurringJobs(new[] {jobId})
                .FirstOrDefault();
        }
    }
}