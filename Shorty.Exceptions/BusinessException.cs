namespace Shorty.Exceptions;

public class BusinessException : CustomBaseException
{
    public BusinessException(string message) : base(message)
    {
    }
}