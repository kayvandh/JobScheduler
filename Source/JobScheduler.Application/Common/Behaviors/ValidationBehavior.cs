using FluentResults;
using FluentValidation;
using Framework.FluentResultsAddOn;
using MediatR;

namespace JobScheduler.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
      where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (!failures.Any())
                return await next();

            var fluentErrors = failures
                .Select(f => FluentError.Raise(ApplicationErrorCode.BadRequest, f.ErrorMessage,
                          new Dictionary<string, object> { ["Property"] = f.PropertyName }))
                .ToList<IError>();

            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse).GenericTypeArguments.First();
                var failMethod = typeof(Result)
                    .GetMethods()
                    .First(m => m.Name == nameof(Result.Fail) &&
                        m.IsGenericMethod &&
                        m.GetParameters().Length == 1
                        && typeof(IEnumerable<IError>).IsAssignableFrom(m.GetParameters()[0].ParameterType))
                    .MakeGenericMethod(resultType);

                var result = (TResponse)failMethod.Invoke(null, new object[] { fluentErrors })!;
                return result;
            }

            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Fail(fluentErrors);
            }

            throw new ValidationException(failures);
        }
    }
}