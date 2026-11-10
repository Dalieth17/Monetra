using FluentValidation;
using Monetra.Application.DTOs.Transaction;

namespace Monetra.Application.Validators.Transaction;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("Debe seleccionar una cuenta válida.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser un valor positivo estrictamente mayor a 0.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de movimiento inválido (debe ser Ingreso o Egreso).");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("La descripción no puede exceder los 255 caracteres.");
    }
}

