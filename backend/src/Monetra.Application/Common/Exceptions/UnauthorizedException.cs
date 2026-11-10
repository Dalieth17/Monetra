namespace Monetra.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "No autorizado para acceder a este recurso.") 
        : base(message)
    {
    }
}

