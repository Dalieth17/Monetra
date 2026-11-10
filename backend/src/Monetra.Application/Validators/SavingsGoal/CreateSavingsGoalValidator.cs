using FluentValidation;
using Monetra.Application.DTOs.SavingsGoal;

namespace Monetra.Application.Validators.SavingsGoal;

public class CreateSavingsGoalValidator : AbstractValidator<CreateSavingsGoalDto>
{
    public CreateSavingsGoalValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la meta es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("El monto objetivo debe ser mayor a 0.");

        RuleFor(x => x.Deadline)
            .GreaterThan(DateTime.UtcNow.Date).WithMessage("La fecha límite debe ser futura.");
    }
}
