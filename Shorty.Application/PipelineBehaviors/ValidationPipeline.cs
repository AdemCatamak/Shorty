using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Shorty.Exceptions;
using ValidationException = Shorty.Exceptions.ValidationException;

namespace Shorty.Application.PipelineBehaviors;

public class ValidationPipeline<TCommand, TCommandResult> : IPipelineBehavior<TCommand, TCommandResult>
{
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationPipeline(IEnumerable<IValidator<TCommand>> validators)
    {
        _validators = validators;
    }

    public Task<TCommandResult> Handle(TCommand request, RequestHandlerDelegate<TCommandResult> next, CancellationToken cancellationToken)
    {
        List<ValidationFailure> validationFailures
            = _validators
                .Select(validator => validator.Validate(request))
                .SelectMany(validationResult => validationResult.Errors)
                .Where(validationFailure => validationFailure != null)
                .ToList();

        if (validationFailures.Any())
        {
            List<ValidationError> errors = validationFailures.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage, f.CustomState))
                .ToList();
            throw new ValidationException(errors);
        }

        return next();
    }
}