namespace Shorty.Exceptions;

public class ValidationException : CustomBaseException
{
    public List<ValidationError> ValidationErrors { get; }

    public ValidationException(List<ValidationError> validationErrors)
        : base(string.Empty)
    {
        ValidationErrors = validationErrors;
    }

    public static ValidationException Create(params ValidationError[] validationErrors)
    {
        return new ValidationException(validationErrors.ToList());
    }
}

public record ValidationError(string Scope, string Message, object? Data = null);