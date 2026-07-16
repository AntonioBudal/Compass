namespace Compass.Domain.Exceptions;

public class InvalidTimeRangeException : DomainException
{
    public InvalidTimeRangeException(string message) : base(message)
    {
    }

    public InvalidTimeRangeException(DateTime start, DateTime end) 
        : base($"O intervalo de tempo é inválido: o término ({end:u}) deve ser posterior ao início ({start:u}).")
    {
    }
}