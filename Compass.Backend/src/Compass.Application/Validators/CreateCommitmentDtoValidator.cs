using Compass.Application.DTOs;
using FluentValidation;

namespace Compass.Application.Validators;

public class CreateCommitmentDtoValidator : AbstractValidator<CreateCommitmentDto>
{
    private static readonly string[] ValidTypes = { "TASK", "EVENT", "HABIT", "NOTE" };

    public CreateCommitmentDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MinimumLength(3).WithMessage("O título deve ter pelo menos 3 caracteres.")
            .MaximumLength(255).WithMessage("O título não pode exceder 255 caracteres.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("O tipo de compromisso é obrigatório.")
            .Must(type => ValidTypes.Contains(type?.ToUpperInvariant()))
            .WithMessage("Tipo inválido. Valores aceitos: TASK, EVENT, HABIT, NOTE.");

        // Regras exclusivas para TASK (Invariante do Domínio: Duração >= 5m)
        When(x => string.Equals(x.Type, "TASK", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.EstimatedDurationMinutes)
                .NotNull().WithMessage("A duração estimada é obrigatória para tarefas.")
                .GreaterThanOrEqualTo(5).WithMessage("Uma tarefa deve ter pelo menos 5 minutos de duração.");

            RuleFor(x => x.EnergyRequired)
                .NotNull().WithMessage("O nível de energia é obrigatório para tarefas.")
                .InclusiveBetween((short)1, (short)3).WithMessage("A energia deve estar entre 1 (Baixa) e 3 (Alta).");
        });

        // Regras exclusivas para EVENT (Prevenção de horários invertidos)
        When(x => string.Equals(x.Type, "EVENT", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.StartTime)
                .NotNull().WithMessage("O horário de início é obrigatório para eventos.");

            RuleFor(x => x.EndTime)
                .NotNull().WithMessage("O horário de término é obrigatório para eventos.")
                .GreaterThan(x => x.StartTime).WithMessage("O término do evento deve ser posterior ao horário de início.");
        });

        // Regras exclusivas para HABIT
        When(x => string.Equals(x.Type, "HABIT", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.CronExpression)
                .NotEmpty().WithMessage("A expressão CRON é obrigatória para hábitos.");
        });
    }
}