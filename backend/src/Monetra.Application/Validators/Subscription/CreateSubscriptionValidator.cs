using FluentValidation;
using Monetra.Application.DTOs.Subscription;

namespace Monetra.Application.Validators.Subscription;

public class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionDto>
{
    private static readonly string[] AllowedCycles = ["Monthly", "Annual", "Weekly"];

    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la suscripción es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto de la suscripción debe ser mayor a 0.");

        RuleFor(x => x.BillingCycle)
            .Must(c => AllowedCycles.Contains(c))
            .WithMessage("Ciclo de cobro inválido. Opciones válidas: Monthly, Annual, Weekly.");
    }
}
