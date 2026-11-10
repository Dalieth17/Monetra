namespace Monetra.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("Se presentaron uno o más errores de validación.")
    {
        Errors = errors;
    }
}
