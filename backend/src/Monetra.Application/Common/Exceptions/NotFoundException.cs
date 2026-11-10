namespace Monetra.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key) 
        : base($"La entidad \"{name}\" ({key}) no fue encontrada.")
    {
    }
}

