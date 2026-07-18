using Compass.Application.DTOs;
using FluentValidation;

namespace Compass.Application.Validators;

public class UpdateStatusDtoValidator : AbstractValidator<UpdateStatusDto>
{
    private static readonly string[] ValidStatuses = { "PENDING", "IN_PROGRESS", "COMPLETED", "ARCHIVED", "BLOCKED" };

    public UpdateStatusDtoValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("O novo status é obrigatório.")
            .Must(status => ValidStatuses.Contains(status?.ToUpperInvariant()))
            .WithMessage("Status inválido. Valores aceitos: PENDING, IN_PROGRESS, COMPLETED, ARCHIVED, BLOCKED.");
    }
}