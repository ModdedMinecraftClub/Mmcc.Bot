using FluentValidation;
using MediatR;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Queries
{
    public class TestQuery
    {
        public record Query(string? TestString) : IRequest<Result>;

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(q => q.TestString)
                    .NotEmpty();
            }
        }
        
        public class Handler : RequestHandler<Query, Result>
        {
            protected override Result Handle(Query request)
            {
                return Result.FromSuccess();
            }
        }
    }
}