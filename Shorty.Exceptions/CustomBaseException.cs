namespace Shorty.Exceptions;

public abstract class CustomBaseException : Exception
{
    public CustomBaseException(string message) : base(message)
    {
    }
}