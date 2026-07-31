using Compass.Application.DTOs;
using FluentValidation;

namespace Compass.Application.Validators;

public class UpdateGoalDtoValidator : AbstractValidator<UpdateGoalDto>
{
    public UpdateGoalDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MinimumLength(3).WithMessage("O título deve possuir pelo menos 3 caracteres.");
    }
}