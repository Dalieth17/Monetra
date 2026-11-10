using FluentValidation;
using Monetra.Application.DTOs.Debt;

namespace Monetra.Application.Validators.Debt;

public class CreateDebtValidator : AbstractValidator<CreateDebtDto>
{
    public CreateDebtValidator()
    {
        RuleFor(x => x.Creditor)
            .NotEmpty().WithMessage("La entidad acreedora es obligatoria.")
            .MaximumLength(100).WithMessage("El acreedor no puede superar los 100 caracteres.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("El capital de la deuda debe ser mayor a 0.");

        RuleFor(x => x.InterestRate)
            .GreaterThanOrEqualTo(0).WithMessage("La tasa de interés no puede ser negativa.");

        RuleFor(x => x.RateType)
            .Must(r => r == "EA" || r == "EM")
            .WithMessage("El tipo de tasa debe ser EA (Efectiva Anual) o EM (Efectiva Mensual).");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("La fecha de vencimiento debe ser futura.");
    }
}

