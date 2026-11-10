using FluentValidation;
using Monetra.Application.DTOs.Account;

namespace Monetra.Application.Validators.Account;

public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
{
    private static readonly string[] AllowedCurrencies = ["COP", "USD", "EUR"];

    public CreateAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la cuenta es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("La divisa es obligatoria.")
            .Must(c => AllowedCurrencies.Contains(c.ToUpper()))
            .WithMessage("Divisa no soportada. Permitidas: COP, USD, EUR.");
    }
}

