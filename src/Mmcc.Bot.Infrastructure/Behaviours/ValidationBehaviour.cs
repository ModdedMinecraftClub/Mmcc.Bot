using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Extensions.System;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Behaviours
{
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var responseType = typeof(TResponse);
            var context = new ValidationContext<TRequest>(request);
            var failures = _validators
                .Select(x => x.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(x => x is not null)
                .ToList();

            // ReSharper disable InvertIf
            if (failures.Any())
            {
                var error = new ValidationError(
                    $"Error while validating '{typeof(TRequest).ToFriendlyDisplayString()}'.", failures);
                var errorResult = responseType
                    .GetMethod("FromError", new[] {Type.MakeGenericMethodParameter(0), typeof(IResult)})?
                    .MakeGenericMethod(typeof(ValidationError))
                    .Invoke(request, new object?[] {error, null});
                return Task.FromResult((TResponse) errorResult!);
            }
        
            return next();
        }
    }
}