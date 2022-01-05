using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Storage;
using MediatR;

namespace Mmcc.Bot.Polychat.Jobs.Recurring.Restarts;

public class GetUpcoming
{
    public record Query : IRequest<IList<QueryResult>>;

    public record QueryResult(string ServerId, RecurringJobDto Job);
    
    public class Handler : RequestHandler<Query, IList<QueryResult>>
    {
        protected override IList<QueryResult> Handle(Query request) =>
            JobStorage.Current
                .GetConnection()
                .GetRecurringJobs()
                .Where(
                    j => j.Id.StartsWith(PolychatJobIdPrefixes.Restart) &&
                         j.NextExecution is not null &&
                         j.NextExecution.Value - DateTime.UtcNow < TimeSpan.FromMinutes(5)
                )
                .Select(j => new QueryResult(j.Id[(j.Id.LastIndexOf("_", StringComparison.Ordinal) + 1)..], j))
                .ToList();
    }
}