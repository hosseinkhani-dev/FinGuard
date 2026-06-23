using FluentValidation;
using MediatR;

namespace FinGuard.Application.Commons;

public class ValidationBehavior<TRequest, TResponse> :
        IPipelineBehavior<TRequest, TResponse>
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
        // 1. Check if there are any validators attached to this Command
        if (!_validators.Any())
        {
            return await next(); 
        }

        // 2. Run all validators
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v =>
            v.ValidateAsync(context, cancellationToken)));

        // 3. Collect errors
        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        // 4. If errors exist, STOP and throw a ValidationException
        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        // 5. If everything is clean, continue to the Handler
        return await next();
    }
}
