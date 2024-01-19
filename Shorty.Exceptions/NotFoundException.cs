namespace Shorty.Exceptions;

public class NotFoundException : CustomBaseException
{
    public NotFoundException(string message) : base(message)
    {
    }
}