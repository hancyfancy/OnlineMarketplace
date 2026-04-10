using FluentValidation;
using MediatR;
using Demo.Domain.Primitives;

namespace Demo.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count == 0)
                return await next();

            var error = new Error("Validation", string.Join("; ", failures.Select(f => f.ErrorMessage)));

            var responseType = typeof(TResponse);

            if (responseType == typeof(Result))
                return (TResponse)(object)Result.Failure(error);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(Result)
                    .GetMethods()
                    .First(m => m.Name == nameof(Result.Failure)
                             && m.IsGenericMethodDefinition
                             && m.GetParameters().Length == 1
                             && m.GetParameters()[0].ParameterType == typeof(Error))
                    .MakeGenericMethod(responseType.GetGenericArguments()[0]);

                return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
            }

            throw new ValidationException(failures);
        }
    }
}
